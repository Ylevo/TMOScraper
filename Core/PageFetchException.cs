using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMOScrapper.Core
{
    public class PageFetchException : Exception
    {
        public PageFetchingResult PageFetchResult { get; private set; }

        public PageFetchException(PageFetchingResult pageFetchResult)
        {
            PageFetchResult = pageFetchResult;
        }
    }
}
