﻿using Newtonsoft.Json.Linq;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TMOScrapper.Properties;

namespace TMOScrapper.Core.PageFetcher
{
    public class PuppeteerPageFetcher : IPageFetcher
    {
        private static IBrowser? browser = null;
        private IResponse? lastResponse = null;
        private readonly NavigationOptions navigationOptionsDefault = new() { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.DOMContentLoaded }, Timeout = 4000 };
        private readonly NavigationOptions navigationOptionsRedirect = new() { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle2 }, Timeout = 2500 };

        public PuppeteerPageFetcher() { }

        public static async Task InitializePuppeteer()
        {
            var browserFetcher = new BrowserFetcher();
            int downloadProgress = -1;
            DateTime downloadTimestamp = DateTime.Now;

            browserFetcher.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e)
            {
                if (e.ProgressPercentage != downloadProgress && (DateTime.Now - downloadTimestamp).TotalSeconds > 2)
                {
                    downloadProgress = e.ProgressPercentage;
                    downloadTimestamp = DateTime.Now;
                    Log.Information($"Progress : {downloadProgress}/100%");
                }
            };

            Log.Information("Downloading Puppeteer.");
            downloadTimestamp = DateTime.Now;
            await browserFetcher.DownloadAsync();
            Log.Information("Done downloading Puppeteer.");
            var extra = new PuppeteerExtra().Use(new StealthPlugin());
            browser = await extra.LaunchAsync(new LaunchOptions { Headless = true });
        }

        public async Task<string> GetPage(string url, CancellationToken token, PageType type = PageType.Default)
        {
            using IPage page = await browser.NewPageAsync();
            await ConfigurePuppeteerPage(page);

            return type switch
            {
                PageType.Default => await GetDefault(url, page, token),
                PageType.Chapter => await GetChapter(url, page, token),
                _ => "",
            };
        }

        private async Task<string> GetDefault(string url, IPage page, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await page.GoToAsync(url);

            return lastResponse.Status switch
            {
                HttpStatusCode.OK => await page.GetContentAsync(),
                HttpStatusCode.Forbidden => throw new PageFetchBannedException(),
                HttpStatusCode.NotFound => throw new PageFetchNotFoundException(),
                HttpStatusCode.TooManyRequests => throw new PageFetchRateLimitedException(),
                _ => throw new PageFetchFailureException(lastResponse.Status),
            };
        }

        private async Task<string> GetChapter(string url, IPage page, CancellationToken token)
        {
            bool goodToGo = true;
            token.ThrowIfCancellationRequested();

            try
            {
                await page.GoToAsync(url, navigationOptionsDefault);

                if (page.Url.Contains("/view_uploads/"))
                {
                    await page.WaitForNavigationAsync(navigationOptionsRedirect);
                }
            }
            catch (NavigationException) { }
            catch (TimeoutException) { }

            switch (lastResponse.Status)
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
                    throw new PageFetchFailureException(lastResponse.Status);
            }

            string chapterId = Regex.Match(page.Url, @"(?<=\/)([a-zA-Z0-9]{32})(?=\/)").Value;

            if (chapterId.Length != 32)
            {
                Log.Error("Failed to reach the page including the chapter ID using puppeteer.");
                throw new PageFetchFailureException(lastResponse.Status);
            }

            if (!page.Url.Contains(Settings.Default.Domain) || !page.Url.Contains("/cascade"))
            {
                url = $"{Settings.Default.Domain}/viewer/{chapterId}/cascade";
                goodToGo = false;
            }

            return goodToGo ? await page.GetContentAsync() : await GetChapter(url, page, token);
        }

        private async Task ConfigurePuppeteerPage(IPage page)
        {
            await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
            {
                { "Referer", Settings.Default.Domain }
            });
            await page.SetJavaScriptEnabledAsync(true);
            await page.SetRequestInterceptionAsync(true);
            await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:108.0) Gecko/20100101 Firefox/108.0");

            page.Response += (sender, e) =>
            {
                lastResponse = e.Response;
            };

            page.Request += (sender, e) =>
            {
                switch (e.Request.ResourceType)
                {
                    case ResourceType.Image:
                    case ResourceType.Img:
                    case ResourceType.StyleSheet:
                    case ResourceType.ImageSet:
                    case ResourceType.Media:
                        e.Request.AbortAsync();
                        break;
                    default:
                        e.Request.ContinueAsync();
                        break;
                }
            };
        }

        public static void CloseBrowser()
        {
            browser?.Dispose();
        }
    }
}