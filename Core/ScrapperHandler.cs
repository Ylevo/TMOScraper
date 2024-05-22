using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TMOScrapper.Core.PageFetcher;
using TMOScrapper.Properties;

namespace TMOScrapper.Core
{
    public enum PageFetcherImplementation { HtmlAgi, Puppeteer };

    internal class ScrapperHandler
    {
        private readonly Scrapper scrapper;
        private PageFetcherImplementation currentImplementation;
        private readonly Dictionary<PageFetcherImplementation, Func<IPageFetcher>> pageFetcherDict = new (){
            { PageFetcherImplementation.HtmlAgi, () => new HtmlAgiPageFetcher() },
            { PageFetcherImplementation.Puppeteer, () => new PuppeteerPageFetcher() }
        };
        public ScrapperHandler(Scrapper scrapper)
        {
            this.scrapper = scrapper;
            currentImplementation = Settings.Default.AlwaysUsePuppeteer ? PageFetcherImplementation.Puppeteer : PageFetcherImplementation.HtmlAgi;
            scrapper.PageFetcher = pageFetcherDict[currentImplementation]();
        }

        public CancellationTokenSource GetTokenSource()
        {
            return scrapper.TokenSource;
        }

        public async Task StartScrapping(
            string url,
            string[]? groups,
            (bool skipChapters, decimal from, decimal to) chapterRange,
            int skipMango)
        {
            try
            {
                if (!await scrapper.ScrapChapters(url, groups, chapterRange, skipMango) && currentImplementation != PageFetcherImplementation.Puppeteer)
                {
                    SwitchToPuppeteer();
                    await scrapper.ScrapChapters(url, groups, chapterRange, skipMango);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Information("Aborted scrapping.");
            }
            catch (Exception ex)
            {
                Log.Error($"An unexpected error occurred : {ex.Message}");
                Log.Error($"Stacktrace : {ex.StackTrace} ");
            }
        }

        public async Task<List<string>?> ScrapScanGroups(string url)
        {
            List<string>? groups = null;

            try
            {
                var tupleResult = await scrapper.ScrapScanGroups(url);
                if (!tupleResult.result && currentImplementation != PageFetcherImplementation.Puppeteer)
                {
                    SwitchToPuppeteer();
                    return await ScrapScanGroups(url);
                }
                groups = tupleResult.groups;
            }
            catch (OperationCanceledException)
            {
                Log.Information("Aborted scrapping.");
            }
            catch (Exception ex)
            {
                Log.Error($"An unexpected error occurred : {ex.Message}");
                Log.Error($"Stacktrace : {ex.StackTrace} ");
            }

            return groups;
        }
        public void SwitchToPuppeteer()
        {
            currentImplementation = PageFetcherImplementation.Puppeteer;
            scrapper.PageFetcher = pageFetcherDict[currentImplementation]();
        }
    }
}
