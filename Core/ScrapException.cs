using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMOScrapper.Core
{
    public class ScrapException : Exception
    {
        public ScrapResult ScrapResult { get; private set; }

        public ScrapException(ScrapResult scrapResult)
        {
            ScrapResult = scrapResult;
        }
    }
}
