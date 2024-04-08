using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMOScrapper.Core.PageFetcher;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace TMOScrapper.Core
{
    internal interface IScrapper
    {
        IPageFetcher? PageFetcher { get; set; }
        CancellationTokenSource? CancellationTokenSource { get; set; }
        Task<ScrapResult> StartScrapping(string url, string[]? groups = null);
        Task<(ScrapResult result, List<string>? groups)> ScrapScanGroups(string url);
    }
}
