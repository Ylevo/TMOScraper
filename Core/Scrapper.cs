using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using TMOScrapper.Core.PageFetcher;
using System.Text.RegularExpressions;
using TMOScrapper.Properties;

namespace TMOScrapper.Core
{
    internal class Scrapper : IScrapper
    {
        public IPageFetcher? PageFetcher { get; set; } = null;
        public CancellationTokenSource? CancellationTokenSource { get; set; } = null;
        private readonly string domainName = Settings.Default.Domain;
        private readonly HtmlDocument doc;

        public Scrapper(HtmlDocument doc) 
        {
            this.doc = doc;
        }

        public async Task<ScrapResult> StartScrapping(string url, string[]? groups = null)
        {
            string pattern = $@"(?<={domainName}\/)
                               ((?<Bulk>library\/(manga|manhua|manhwa|doujinshi|one_shot)\/)
                               |(?<Single>view_uploads|viewer\/)
                               |(?<Group>groups\/(.*)proyects))";
            string pageType = Regex.Match(url, pattern, RegexOptions.ExplicitCapture).Groups.Values.Where(g => g.Success && g.Name != "0").FirstOrDefault()?.Name ?? "";
            ScrapResult result;
            switch (pageType)
            {
                case "Bulk":
                    result = await ScrapBulkChapters();
                    break;
                case "Single":
                    result = await ScrapSingleChapter();
                    break;
                case "Group":
                    result = await ScrapGroupChapters();
                    break;
                default:
                    result = ScrapResult.NotFound;
                    //AddLog("Error: wrong URL.");
                    break;
            }
            return result;
        }

        public async Task<(ScrapResult result, List<string>? groups)> ScrapScanGroups(string url)
        {
            List<string>? scanGroups = null;
            string html = ""; 

            try
            {
                html = await PageFetcher.GetPage(url, CancellationTokenSource.Token);
            }
            catch(ScrapException ex)
            {
                //log
                return (ex.ScrapResult, scanGroups);
            }

            var scanGroupsNodes = ParseScanGroups(html);

            if (scanGroupsNodes == null)
            {
                //log
                return (ScrapResult.NotFound, scanGroups);
            }

            foreach (var scanGroupNode in scanGroupsNodes)
            {
                scanGroups.Add(String.Join('+', scanGroupNode.ParentNode.InnerText.Split(',', StringSplitOptions.TrimEntries)));
            }

            scanGroups = scanGroups.Distinct().ToList();
            scanGroups.Sort();

            return (ScrapResult.Success, scanGroups);
        }

        private Task<ScrapResult> ScrapSingleChapter()
        {
            
        }

        private Task<ScrapResult> ScrapBulkChapters()
        {

        }

        private Task<ScrapResult> ScrapGroupChapters()
        {

        }

       private SortedDictionary<string, (string, string)[]> GetChaptersLinks(HtmlNodeCollection nodes)
        {

        }

        private SortedDictionary<string, (string, string)[]> GetOneShotLinks(HtmlNodeCollection nodes)
        {

        }

        private HtmlNodeCollection ParseScanGroups(string html)
        {
            doc.LoadHtml(html);
            return doc.DocumentNode.SelectNodes(@"//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]
                                                                 //div[1][contains(concat(' ',normalize-space(@class),' '),' text-truncate ')]
                                                                 /span");
        }
        

        private HtmlNodeCollection ParseChaptersLinks(string html)
        {
            doc.LoadHtml(html);
            return doc.DocumentNode.SelectNodes("//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]");
        }

        private HtmlNodeCollection ParseChapterImages(string html)
        {
            doc.LoadHtml(html);
            return doc.DocumentNode.SelectNodes("//img[contains(concat(' ',normalize-space(@class),' '),' viewer-img ')]");
        }

    }
}
