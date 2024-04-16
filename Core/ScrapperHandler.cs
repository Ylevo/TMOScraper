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
    public enum ScrapResult { Success, PartialSuccess, Failure, Banned, NotFound, Aborted, RateLimited }
    internal class ScrapperHandler
    {
        private readonly IScrapper scrapper;
        private PageFetcherImplementation currentImplementation;
        private Dictionary<PageFetcherImplementation, Func<IPageFetcher?>> pageFetcherDict = new (){
            { PageFetcherImplementation.HtmlAgiPageFetcher, () => new HtmlAgiPageFetcher() },
            { PageFetcherImplementation.PuppeteerPageFetcher, () => new PuppeteerPageFetcher() }
        };

        public CancellationTokenSource CancellationTokenSource { get; init; }
        public ScrapperHandler(IScrapper scrapper, bool usePuppeteer = false)
        {
            this.scrapper = scrapper;
            CancellationTokenSource = new();
            currentImplementation = usePuppeteer ? PageFetcherImplementation.PuppeteerPageFetcher : PageFetcherImplementation.HtmlAgiPageFetcher;
            scrapper.PageFetcher = pageFetcherDict[currentImplementation]();
            scrapper.CancellationTokenSource = CancellationTokenSource;
        }

        public async Task StartScrapping(string url, string[]? groups = null)
        {
            try
            {
                if (await scrapper.StartScrapping(url, groups) == ScrapResult.Failure && currentImplementation != PageFetcherImplementation.PuppeteerPageFetcher)
                {
                    SwitchToPuppeteer();
                    await StartScrapping(url, groups);
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
                var result = await scrapper.ScrapScanGroups(url);
                if (result.result == ScrapResult.Failure && currentImplementation != PageFetcherImplementation.PuppeteerPageFetcher)
                {
                    SwitchToPuppeteer();
                    return await ScrapScanGroups(url);
                }
                groups = result.groups;
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
