using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMOScraper.Core.PageFetcher
{
    public enum PageType { Default, Chapter }
    public interface IPageFetcher
    {
        Task<string> GetPage(string url, CancellationToken token, PageType page = PageType.Default);
    }
}
