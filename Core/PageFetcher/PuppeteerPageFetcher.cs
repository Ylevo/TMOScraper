using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMOScrapper.Core.PageFetcher
{
    internal class PuppeteerPageFetcher : IPageFetcher
    {
        public PuppeteerPageFetcher() { }

        public async Task<string> GetPage(string url, CancellationToken token, PageType page = PageType.Default)
        {
            return "";
        }
    }
}
