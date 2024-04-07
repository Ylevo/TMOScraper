using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMOScrapper.Core.PageFetcher;

namespace TMOScrapper.Core
{
    internal class Scrapper : IScrapper
    {
        public IPageFetcher? PageFetcher { get; set; }
        public Scrapper() { }

        public async Task<ScrapResult> StartScrapping(string url)
        {
            return ScrapResult.Success;
        }

        public async Task<(ScrapResult result, List<string>? groups)> ScrapScanGroups(string url)
        {
            return (ScrapResult.Success, null);
        }



    }
}
