using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMOScrapper.Core.PageFetcher
{
    public interface IPageFetcher
    {
        string GetPage(string url);
    }
}
