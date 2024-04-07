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
        Task<ScrapResult> StartScrapping(string url);
        Task<(ScrapResult result, List<string>? groups)> ScrapScanGroups(string url);
        /*
        Task ScrapSingleChapter();
        Task ScrapBulkChapters();
        Task ScrapScanGroupChapters();
        HtmlNodeCollection ParseScanGroups(HtmlDocument doc);
        SortedDictionary<string, (string, string)[]> GetChaptersLinks(HtmlNodeCollection nodes);
        SortedDictionary<string, (string, string)[]> GetOneShotLinks(HtmlNodeCollection nodes);
        HtmlNodeCollection ParseChaptersLinks(HtmlDocument doc);*/
    }
}
