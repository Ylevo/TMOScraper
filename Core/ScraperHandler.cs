using Serilog;
using TMOScraper.Core.PageFetcher;
using TMOScraper.Properties;

namespace TMOScraper.Core
{
    public enum PageFetcherImplementation { HtmlAgi, Puppeteer };
    public enum ScrapingResult { Success, PageFetchingFailure, NoGroupSelected, PageNotFound };

    internal class ScraperHandler
    {
        private readonly Scraper scraper;
        private PageFetcherImplementation currentImplementation;
        private readonly Dictionary<PageFetcherImplementation, Func<IPageFetcher>> pageFetcherDict = new (){
            { PageFetcherImplementation.HtmlAgi, () => new HtmlAgiPageFetcher() },
            { PageFetcherImplementation.Puppeteer, () => new PuppeteerPageFetcher() }
        };
        public ScraperHandler(Scraper scraper)
        {
            this.scraper = scraper;
            currentImplementation = Settings.Default.AlwaysUsePuppeteer ? PageFetcherImplementation.Puppeteer : PageFetcherImplementation.HtmlAgi;
            scraper.PageFetcher = pageFetcherDict[currentImplementation]();
        }

        public CancellationTokenSource GetTokenSource()
        {
            return scraper.TokenSource;
        }

        public async Task ScrapChapters(
            string url,
            string[]? groups,
            (bool skipChapters, decimal from, decimal to) chapterRange,
            int skipMango)
        {
            try
            {
                ScrapingResult result = await scraper.ScrapChapters(url, groups, chapterRange, skipMango);

                if (TryWithPuppeteer(result))
                {
                    await ScrapChapters(url, groups, chapterRange, skipMango);
                    return;
                }

                LogScrapingResult(result);
            }
            catch (OperationCanceledException)
            {
                Log.Information("Aborted scraping.");
            }
            catch (Exception ex)
            {
                Log.Error($"An unexpected error occurred : {ex.Message}");
                Log.Fatal($"Stacktrace : {ex.StackTrace} ");
            }
        }

        public async Task<List<string>?> ScrapScanGroups(string url)
        {
            List<string>? groups = null;

            try
            {
                var tupleResult = await scraper.ScrapScanGroups(url);

                if (TryWithPuppeteer(tupleResult.result))
                {
                    return await ScrapScanGroups(url);
                }

                groups = tupleResult.groups;
                LogScrapingResult(tupleResult.result);
            }
            catch (OperationCanceledException)
            {
                Log.Information("Aborted scraping.");
            }
            catch (Exception ex)
            {
                Log.Error($"An unexpected error occurred : {ex.Message}");
                Log.Fatal($"Stacktrace : {ex.StackTrace} ");
            }

            return groups;
        }

        private bool TryWithPuppeteer(ScrapingResult result)
        {
            bool failed = result == ScrapingResult.PageFetchingFailure && currentImplementation != PageFetcherImplementation.Puppeteer;

            if (failed)
            {
                Log.Information("Switching to puppeteer implementation.");
                currentImplementation = PageFetcherImplementation.Puppeteer;
                scraper.PageFetcher = pageFetcherDict[currentImplementation]();
            }

            return failed;
        }

        private void LogScrapingResult(ScrapingResult result)
        {
            switch(result)
            {
                case ScrapingResult.Success:
                    Log.Information("Scraping ended successfully.");
                    break;
                case ScrapingResult.PageNotFound:
                    Log.Error("Scraping failed : wrong URL or page not found.");
                    break;
                case ScrapingResult.PageFetchingFailure:
                    Log.Error("Scraping failed : couldn't fetch the page(s). Current implementation might be broken or you're banned/ratelimited.");
                    break;
                case ScrapingResult.NoGroupSelected:
                    Log.Error("Scraping failed : no group selected.");
                    break;
            };
        }
    }
}
