using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TMOScrapper.Core.PageFetcher;

namespace TMOScrapper.Core
{
    public enum PageFetcherImplementation { HtmlAgi, Puppeteer };
    public enum PageFetchingResult { Success, Failure, Banned, NotFound, Aborted, RateLimited }
    internal class ScrapperHandler
    {
        private readonly Scrapper scrapper;
        private PageFetcherImplementation currentImplementation;
        private readonly Dictionary<PageFetcherImplementation, Func<IPageFetcher>> pageFetcherDict = new (){
            { PageFetcherImplementation.HtmlAgi, () => new HtmlAgiPageFetcher() },
            { PageFetcherImplementation.Puppeteer, () => new PuppeteerPageFetcher() }
        };

        public CancellationTokenSource CancellationTokenSource { get; init; }
        public ScrapperHandler(Scrapper scrapper, bool usePuppeteer = false)
        {
            this.scrapper = scrapper;
            CancellationTokenSource = new();
            currentImplementation = usePuppeteer ? PageFetcherImplementation.Puppeteer : PageFetcherImplementation.HtmlAgi;
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
                if (!await scrapper.ScrapChapters(url, groups, chapterRange, skipMango) && currentImplementation != PageFetcherImplementation.Puppeteer)
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
                if (!tupleResult.result && currentImplementation != PageFetcherImplementation.Puppeteer)
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
            currentImplementation = PageFetcherImplementation.Puppeteer;
            scrapper.PageFetcher = pageFetcherDict[currentImplementation]();
        }
    }
}
