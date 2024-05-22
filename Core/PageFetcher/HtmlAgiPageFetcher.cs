using HtmlAgilityPack;
using PuppeteerSharp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TMOScrapper.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Serilog;

namespace TMOScrapper.Core.PageFetcher
{
    public class HtmlAgiPageFetcher : IPageFetcher
    {
        private HtmlDocument document = new();
        private readonly HtmlWeb webClient = new();

        public HtmlAgiPageFetcher() 
        {
            webClient.PreRequest += delegate (HttpWebRequest req)
            {
                req.Referer = Settings.Default.Domain;
                return true;
            };
        }

        public async Task<string> GetPage(string url, CancellationToken token, PageType type = PageType.Default)
        {
            return type switch
            {
                PageType.Default => GetDefault(url, token),
                PageType.Chapter => await GetChapter(url, token),
                _ => "",
            };
        }

        private string GetDefault(string url, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            // LoadFromWebAsync has terrible implementation for some reason
            document = webClient.Load(url);

            return webClient.StatusCode switch
            {
                HttpStatusCode.OK => document.ParsedText,
                HttpStatusCode.Forbidden => throw new PageFetchBannedException(),
                HttpStatusCode.NotFound => throw new PageFetchNotFoundException(),
                HttpStatusCode.TooManyRequests => throw new PageFetchRateLimitedException(),
                _ => throw new PageFetchFailureException(webClient.StatusCode),
            };
        }

        private async Task<string> GetChapter(string url, CancellationToken token)
        {
            bool goodToGo = true;

            token.ThrowIfCancellationRequested();

            document = webClient.Load(url);

            switch (webClient.StatusCode)
            {
                case HttpStatusCode.OK:
                    break;
                case HttpStatusCode.Forbidden:
                    throw new PageFetchBannedException();
                case HttpStatusCode.NotFound:
                    throw new PageFetchNotFoundException();
                case HttpStatusCode.TooManyRequests:
                    throw new PageFetchRateLimitedException();
                default:
                    throw new PageFetchFailureException(webClient.StatusCode);
            }

            string returnedUri = webClient.ResponseUri.AbsoluteUri;

            if (returnedUri.Contains("/view_uploads/"))
            {
                string chapterId = Regex.Match(document.ParsedText, @"(?<=uniqid:.*'\b)(.*)(?=')").Value;
                if (chapterId.Length == 0)
                {
                    Log.Error("Could not retrieve chapter id to fetch the chapter page.");
                    throw new PageFetchFailureException(webClient.StatusCode);
                }
                else
                {
                    url = $"{Settings.Default.Domain}/viewer/{chapterId}/cascade";
                }
                goodToGo = false;
                await Task.Delay(800, token);
            }
            if (returnedUri.Contains("/paginated"))
            {
                url = Regex.Replace(returnedUri, "/paginated.*", "/cascade");
                goodToGo = false;
                await Task.Delay(800, token);
            }

            return goodToGo ? document.ParsedText : await GetChapter(url, token);
        }
    }
}
