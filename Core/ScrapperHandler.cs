using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TMOScrapper.Core.PageFetcher;

namespace TMOScrapper.Core
{
    enum PageFetcherImplementation { HtmlAgiPageFetcher, PuppeteerPageFetcher };
    enum ScrapResult { Success, Failure, Banned }
    internal class ScrapperHandler
    {
        private readonly IScrapper scrapper;
        private PageFetcherImplementation currentImplementation;
        private Dictionary<PageFetcherImplementation, Func<IPageFetcher?>> pageFetcherDict = new (){
            { PageFetcherImplementation.HtmlAgiPageFetcher, () => new HtmlAgiPageFetcher() },
            { PageFetcherImplementation.PuppeteerPageFetcher, () => new PuppeteerPageFetcher() }
        };
        public ScrapperHandler(IScrapper scrapper, bool usePuppeteer = false)
        {
            this.scrapper = scrapper;
            currentImplementation = usePuppeteer ? PageFetcherImplementation.PuppeteerPageFetcher : PageFetcherImplementation.HtmlAgiPageFetcher;
            scrapper.PageFetcher = pageFetcherDict[currentImplementation]();
        }

        public async Task StartScrapping(string url)
        {
            if (await scrapper.StartScrapping(url) == ScrapResult.Failure && currentImplementation != PageFetcherImplementation.PuppeteerPageFetcher)
            {
                SwitchToPuppeteer();
                await StartScrapping(url);
            }
        }

        public async Task<List<string>?> ScrapScanGroups(string url)
        {
            var result = await scrapper.ScrapScanGroups(url);
            if (result.result == ScrapResult.Failure && currentImplementation != PageFetcherImplementation.PuppeteerPageFetcher)
            {
                SwitchToPuppeteer();
                return await ScrapScanGroups(url);
            }

            return result.groups;
        }
        public void SwitchToPuppeteer()
        {
            currentImplementation = PageFetcherImplementation.PuppeteerPageFetcher;
            scrapper.PageFetcher = pageFetcherDict[currentImplementation]();
        }
    }
}
