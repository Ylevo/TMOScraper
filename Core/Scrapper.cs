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
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Security.Policy;

namespace TMOScrapper.Core
{
    internal class Scrapper : IScrapper
    {
        public IPageFetcher? PageFetcher { get; set; } = null;
        public CancellationTokenSource? CancellationTokenSource { get; set; } = null;
        private readonly string domainName = Settings.Default.Domain;
        private readonly HtmlDocument doc;
        private ResiliencePipeline retryPipeline;


        public Scrapper(HtmlDocument document) 
        {
            doc = document;
            retryPipeline = SetRetryPipeline();
        }

        public async Task<ScrapResult> StartScrapping(
            string url, 
            string[]? groups = null, 
            (bool skipChapters, int chapterFrom, int chapterTo)? chapterRange = null,
            int skipMango = 0)
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
                    result = await ScrapBulkChapters(url, groups, chapterRange);
                    break;
                case "Single":
                    result = await ScrapSingleChapter(url);
                    break;
                case "Group":
                    result = await ScrapGroupChapters(url, skipMango);
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

            try
            {
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, CancellationTokenSource.Token));
            }
            catch(ScrapException ex)
            {
                //log
                return (ex.ScrapResult, scanGroups);
            }

            var scanGroupsNodes = ParseScanGroups(doc);

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

        private Task<ScrapResult> ScrapSingleChapter(string url)
        {
            
        }

        private Task<ScrapResult> ScrapBulkChapters(string url, string[] groups, (bool skipChapters, int chapterFrom, int chapterTo)? chapterRange, bool groupScrapping = false)
        {

        }

        private Task<ScrapResult> ScrapGroupChapters(string url, int skipMango)
        {

        }

        private SortedDictionary<string, (string, string)[]> GetChaptersLinks(HtmlNodeCollection nodes)
        {

        }

        private SortedDictionary<string, (string, string)[]> GetOneShotLinks(HtmlNodeCollection nodes)
        {

        }

        private HtmlNodeCollection ParseScanGroups(HtmlDocument doc)
        {
            return doc.DocumentNode.SelectNodes(@"//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]
                                                                 //div[1][contains(concat(' ',normalize-space(@class),' '),' text-truncate ')]
                                                                 /span");
        }
        

        private HtmlNodeCollection ParseChaptersLinks(HtmlDocument doc)
        {
            return doc.DocumentNode.SelectNodes("//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]");
        }

        private HtmlNodeCollection ParseChapterImages(HtmlDocument doc)
        {
            return doc.DocumentNode.SelectNodes("//img[contains(concat(' ',normalize-space(@class),' '),' viewer-img ')]");
        }

        private ResiliencePipeline SetRetryPipeline()
        {
            var retryOptions = new Polly.Retry.RetryStrategyOptions()
            {
                ShouldHandle = new PredicateBuilder().Handle<ScrapException>(),
                BackoffType = DelayBackoffType.Constant,
                UseJitter = true,
                MaxRetryAttempts = Settings.Default.MaxRetries,
                Delay = TimeSpan.FromMilliseconds(Settings.Default.RetryDelay),
                OnRetry = static args =>
                {
                    var exception = args.Outcome.Exception;
                    if (exception is ScrapException scrapException)
                    {
                        switch (scrapException.ScrapResult)
                        {
                            case ScrapResult.Banned:
                            case ScrapResult.NotFound:
                                args.Outcome.ThrowIfException();
                                break;
                            default:
                                break;
                        }
                    }

                    return default;
                }
            };
            return new ResiliencePipelineBuilder().AddRetry(retryOptions).Build();
        }

    }
}
