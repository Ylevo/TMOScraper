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

namespace TMOScrapper.Core.PageFetcher
{
    internal class HtmlAgiPageFetcher : IPageFetcher
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

        public async Task<string> GetPage(string url, CancellationToken token, PageType page = PageType.Default)
        {
            return page switch
            {
                PageType.Default => await GetDefault(url, token),
                PageType.Chapter => await GetChapter(url, token),
                _ => "",
            };
        }

        private async Task<string> GetDefault(string url, CancellationToken token)
        {

            token.ThrowIfCancellationRequested();
            document = await webClient.LoadFromWebAsync(url, token);

            return webClient.StatusCode switch
            {
                HttpStatusCode.OK => document.ParsedText,
                HttpStatusCode.Forbidden => throw new PageFetchException(PageFetchingResult.Banned),
                HttpStatusCode.NotFound => throw new PageFetchException(PageFetchingResult.NotFound),
                HttpStatusCode.TooManyRequests => throw new PageFetchException(PageFetchingResult.RateLimited),
                _ => throw new PageFetchException(PageFetchingResult.Failure)
            };
        }

        private async Task<string> GetChapter(string url, CancellationToken token)
        {
            bool goodToGo = true;

            token.ThrowIfCancellationRequested();

            document = await webClient.LoadFromWebAsync(url, token);

            switch (webClient.StatusCode)
            {
                case HttpStatusCode.OK:
                    break;
                case HttpStatusCode.Forbidden:
                    throw new PageFetchException(PageFetchingResult.Banned);
                case HttpStatusCode.NotFound:
                    throw new PageFetchException(PageFetchingResult.NotFound);
                case HttpStatusCode.TooManyRequests:
                    throw new PageFetchException(PageFetchingResult.RateLimited);
                default:
                    throw new PageFetchException(PageFetchingResult.Failure);
            }

            string returnedUri = webClient.ResponseUri.AbsoluteUri;

            if (returnedUri.Contains("/view_uploads/"))
            {
                string chapterId = Regex.Match(document.ParsedText, @"(?<=uniqid:.*'\b)(.*)(?=')").Value;
                if (chapterId.Length == 0)
                {
                    // log
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
