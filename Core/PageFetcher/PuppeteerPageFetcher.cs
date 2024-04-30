using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TMOScrapper.Core.PageFetcher
{
    internal class PuppeteerPageFetcher : IPageFetcher
    {
        private static IBrowser? browser = null;
        private readonly NavigationOptions navigationOptionsDefault = new() { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.DOMContentLoaded }, Timeout = 6000 };
        public PuppeteerPageFetcher() 
        {

        }

        public async Task<string> GetPage(string url, CancellationToken token, PageType page = PageType.Default)
        {
            throw new NotImplementedException();
        }

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

        /*private async Task ConfigurePuppeteerPage(IPage page)
        {
            await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
            {
                { "Referer", DomainName }
            });
            await page.SetJavaScriptEnabledAsync(false);
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
                    case ResourceType.Script:
                        e.Request.AbortAsync();
                        break;
                    default:
                        e.Request.ContinueAsync();
                        break;
                }
            };
        }*/

        public static void CloseBrowser()
        {
            browser?.Dispose();
        }
    }
}
