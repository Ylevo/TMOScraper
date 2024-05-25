using Serilog;
using TMOScrapper.Core.PageFetcher;
using TMOScrapper.Properties;

namespace TMOScrapper.Core
{
    public enum PageFetcherImplementation { HtmlAgi, Puppeteer };
    public enum ScrappingResult { Success, PageFetchingFailure, NoGroupSelected, PageNotFound };

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

        public async Task ScrapChapters(
            string url,
            string[]? groups,
            (bool skipChapters, decimal from, decimal to) chapterRange,
            int skipMango)
        {
            try
            {
                ScrappingResult result = await scrapper.ScrapChapters(url, groups, chapterRange, skipMango);

                if (result == ScrappingResult.PageFetchingFailure && currentImplementation != PageFetcherImplementation.Puppeteer)
                {
                    SwitchToPuppeteer();
                    await ScrapChapters(url, groups, chapterRange, skipMango);
                    return;
                }

                LogScrappingResult(result);
            }
            catch (OperationCanceledException)
            {
                Log.Information("Aborted scrapping.");
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
                var tupleResult = await scrapper.ScrapScanGroups(url);

                if (tupleResult.result == ScrappingResult.PageFetchingFailure && currentImplementation != PageFetcherImplementation.Puppeteer)
                {
                    SwitchToPuppeteer();
                    return await ScrapScanGroups(url);
                }

                groups = tupleResult.groups;
                LogScrappingResult(tupleResult.result);
            }
            catch (OperationCanceledException)
            {
                Log.Information("Aborted scrapping.");
            }
            catch (Exception ex)
            {
                Log.Error($"An unexpected error occurred : {ex.Message}");
                Log.Fatal($"Stacktrace : {ex.StackTrace} ");
            }

            return groups;
        }
        private void SwitchToPuppeteer()
        {
            Log.Information("Switching to puppeteer implementation.");
            currentImplementation = PageFetcherImplementation.Puppeteer;
            scrapper.PageFetcher = pageFetcherDict[currentImplementation]();
        }

        private void LogScrappingResult(ScrappingResult result)
        {
            switch(result)
            {
                case ScrappingResult.Success:
                    Log.Information("Scrapping ended successfully.");
                    break;
                case ScrappingResult.PageNotFound:
                    Log.Error("Scrapping failed : wrong URL or page not found.");
                    break;
                case ScrappingResult.PageFetchingFailure:
                    Log.Error("Scrapping failed : couldn't fetch the page(s). Current implementation might be broken or you're banned/ratelimited.");
                    break;
                case ScrappingResult.NoGroupSelected:
                    Log.Error("Scrapping failed : no group selected.");
                    break;
            };
        }
    }
}
