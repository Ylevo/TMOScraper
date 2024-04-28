using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TMOScrapper.Core.PageFetcher;

namespace TMOScrapper.Core
{
    public enum PageFetcherImplementation { HtmlAgiPageFetcher, PuppeteerPageFetcher };
    public enum PageFetchingResult { Success, Failure, Banned, NotFound, Aborted, RateLimited }
    internal class ScrapperHandler
    {
        private readonly Scrapper scrapper;
        private PageFetcherImplementation currentImplementation;
        private Dictionary<PageFetcherImplementation, Func<IPageFetcher?>> pageFetcherDict = new (){
            { PageFetcherImplementation.HtmlAgiPageFetcher, () => new HtmlAgiPageFetcher() },
            { PageFetcherImplementation.PuppeteerPageFetcher, () => new PuppeteerPageFetcher() }
        };

        public CancellationTokenSource CancellationTokenSource { get; init; }
        public ScrapperHandler(Scrapper scrapper, bool usePuppeteer = false)
        {
            this.scrapper = scrapper;
            CancellationTokenSource = new();
            currentImplementation = usePuppeteer ? PageFetcherImplementation.PuppeteerPageFetcher : PageFetcherImplementation.HtmlAgiPageFetcher;
            scrapper.PageFetcher = pageFetcherDict[currentImplementation]();
            scrapper.CancellationTokenSource = CancellationTokenSource;
        }

        public async Task StartScrapping(
            string url,
            string[]? groups,
            (bool skipChapters, int from, int to) chapterRange,
            int skipMango)
        {
            try
            {
                if (!await scrapper.ScrapChapters(url, groups, chapterRange, skipMango) && currentImplementation != PageFetcherImplementation.PuppeteerPageFetcher)
                {
                    SwitchToPuppeteer();
                    await scrapper.ScrapChapters(url, groups, chapterRange, skipMango);
                }
            }
            catch (OperationCanceledException)
            {
                //log
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<List<string>?> ScrapScanGroups(string url)
        {
            List<string>? groups = null;
            try
            {
                var tupleResult = await scrapper.ScrapScanGroups(url);
                if (!tupleResult.result && currentImplementation != PageFetcherImplementation.PuppeteerPageFetcher)
                {
                    SwitchToPuppeteer();
                    return await ScrapScanGroups(url);
                }
                groups = tupleResult.groups;
            }
            catch (OperationCanceledException)
            {
                //log
            }
            catch (Exception ex)
            {

            }
            return groups;
        }
        public void SwitchToPuppeteer()
        {
            currentImplementation = PageFetcherImplementation.PuppeteerPageFetcher;
            scrapper.PageFetcher = pageFetcherDict[currentImplementation]();
        }
    }
}
