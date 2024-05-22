using Polly;
using Serilog;
using System.Text.RegularExpressions;
using TMOScrapper.Core.PageFetcher;
using TMOScrapper.Properties;
using TMOScrapper.Utils;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace TMOScrapper.Core
{
    public class Scrapper
    {
        public IPageFetcher? PageFetcher { get; set; } = null;
        public CancellationTokenSource? TokenSource { get; init; }
        private HtmlParser parser;
        private readonly string domainName = Settings.Default.Domain;
        private readonly HtmlDocument doc;
        private ResiliencePipeline retryPipeline;
        private int toSkipMango = 0;
        private string mainFolder;
        private readonly string language;
        private const string FolderNameTemplate = "{0} [{1}] - {2} [{3}]";


        public Scrapper(CancellationTokenSource tokenSource, HtmlDocument document, HtmlParser htmlParser) 
        {
            TokenSource = tokenSource;
            doc = document;
            parser = htmlParser;
            mainFolder = Settings.Default.MainFolder;
            language = Settings.Default.Language;
            retryPipeline = SetRetryPipeline();
        }

        public async Task<bool> ScrapChapters(
            string url, 
            string[]? groups, 
            (bool skipChapters, decimal from, decimal to) chapterRange,
            int skipMango)
        {
            toSkipMango = toSkipMango < skipMango ? skipMango : toSkipMango;
            string pattern = $@"(?<={domainName}\/)((?<Bulk>library\/(manga|manhua|manhwa|doujinshi|one_shot)\/)|(?<Single>view_uploads|viewer\/)|(?<Group>groups\/(.*)proyects))";
            string pageType = Regex.Match(url, pattern, RegexOptions.ExplicitCapture).Groups.Values.Where(g => g.Success && g.Name != "0").FirstOrDefault()?.Name ?? "";
            switch (pageType)
            {
                case "Bulk":
                    return await ScrapBulkChapters(url, groups, chapterRange);
                case "Single":
                    return await ScrapSingleChapter(url);
                case "Group":
                    return await ScrapGroupChapters(url, skipMango, chapterRange);
                default:
                    Log.Error("Wrong URL : couldn't match it with anything.");
                    return true;
            }
        }

        public async Task<(bool result, List<string>? groups)> ScrapScanGroups(string url)
        {
            try
            {
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, TokenSource.Token));
            }
            catch(PageFetchException ex)
            {
                Log.Error(ex.Message);
                return (false, null);
            }

            List<string> scanGroups = parser.ParseScanGroups(doc);

            if (scanGroups == null || scanGroups.Count == 0)
            {
                Log.Error("404 scannies not found. Check your URL.");
                return (false, null);
            }

            return (true, scanGroups);
        }

        public async Task<bool> ScrapSingleChapter(string url)
        {
            string mangoTitle,
                chapterNumber,
                groupName,
                currentFolder = "";
            List<string> imgUrls;

            try
            {
                Log.Information("Downloading single chapter.");
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token, PageType.Chapter); }, TokenSource.Token));

                chapterNumber = parser.ParseChapterNumberFromChapterPage(doc);
                groupName = parser.ParseGroupNameFromChapterPage(doc);
                mangoTitle = parser.ParseMangoTitleFromChapterPage(doc);
                imgUrls = parser.ParseChapterImages(doc);

                if (Settings.Default.SubFolder)
                {
                    mainFolder = Path.Combine(mainFolder, mangoTitle);
                    Directory.CreateDirectory(mainFolder);
                }

                currentFolder = Path.Combine(mainFolder, String.Format(FolderNameTemplate, mangoTitle, language, chapterNumber, groupName));

                if (Directory.Exists(currentFolder))
                {
                    Log.Warning($"Skipping chapter {chapterNumber} by \"{groupName}\". Folder already exists.");
                }
                else
                {
                    Directory.CreateDirectory(currentFolder);

                    Log.Information($"Download chapter {chapterNumber} by \"{groupName}\"");
                    await Downloader.DownloadChapter(currentFolder, imgUrls, TokenSource.Token);
                    Log.Information($"Done downloading chapter {chapterNumber} by \"{groupName}\"");
                }
                return true;
            }
            catch (PageFetchException ex)
            {
                Log.Error(ex.Message);
                return false;
            }
            finally
            {
                if (TokenSource.IsCancellationRequested && currentFolder != "" && Directory.Exists(currentFolder))
                {
                    Directory.Delete(currentFolder, true);
                }
            }
        }

        public async Task<bool> ScrapBulkChapters(string url, string[]? groups, (bool skipChapters, decimal from, decimal to) chapterRange, bool groupScrapping = false)
        {
            string mangoTitle = "",
               chapterNumber,
               currentFolder = "";
            bool actuallyDidSomething = false,
                isOneShot = url.Contains("/one_shot/");
            List<string> imgUrls;

            try
            {
                if (groups == null)
                {
                    Log.Error("No scannies selected, aborting scrapping.");
                    return true;
                }

                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, TokenSource.Token));
                mangoTitle = parser.ParseMangoTitleFromMangoPage(doc);

                Log.Information($"Scrapping chapters of \"{mangoTitle}\"");

                SortedDictionary<string, (string groupName, string chapterLink)[]> chapters = isOneShot ? parser.ParseOneShotLinks(doc) : parser.ParseChaptersLinks(doc);

                if (chapterRange.skipChapters && !isOneShot)
                {

                    chapters = new SortedDictionary<string, (string groupName, string chapterLink)[]>
                                (chapters.Where(d => decimal.Parse(d.Key) >= chapterRange.from && decimal.Parse(d.Key) <= chapterRange.to)
                                .ToDictionary(d => d.Key, d => d.Value));
                }

                if (Settings.Default.SubFolder)
                {
                    mainFolder = Path.Combine(mainFolder, mangoTitle);
                    Directory.CreateDirectory(mainFolder);
                }

                foreach (var chapter in chapters)
                {
                    actuallyDidSomething = false;
                    chapterNumber = isOneShot ? "000" : "c" + chapter.Key;
                    Log.Information($"Scrapping chapter {chapterNumber}");

                    foreach (var (groupName, chapterLink) in chapter.Value)
                    {
                        currentFolder = "";

                        TokenSource.Token.ThrowIfCancellationRequested();

                        if (groups.Contains(groupName) || (groupScrapping && groups.Any(groupName.Contains)))
                        {
                            currentFolder = Path.Combine(mainFolder, String.Format(FolderNameTemplate, mangoTitle, language, chapterNumber, groupName));

                            if (Directory.Exists(currentFolder))
                            {
                                Log.Warning($"Skipping chapter {chapterNumber} by \"{groupName}\". Folder already exists.");
                                continue;
                            }

                            doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(chapterLink, token, PageType.Chapter); }, TokenSource.Token));
                            imgUrls = parser.ParseChapterImages(doc);
                            Directory.CreateDirectory(currentFolder);

                            Log.Information($"Download chapter {chapterNumber} by \"{groupName}\"");
                            await Downloader.DownloadChapter(currentFolder, imgUrls, TokenSource.Token);
                            Log.Information($"Done downloading chapter {chapterNumber} by \"{groupName}\"");

                            Log.Information($"Waiing {Settings.Default.ChapterDelay} ms ...");
                            await Task.Delay(Settings.Default.ChapterDelay);

                            actuallyDidSomething = true;
                        }
                    }

                    if (!actuallyDidSomething)
                    {
                        Log.Information("No upload found or chapter already downloaded.");
                    }
                }

                Log.Information($"Done scrapping chapters of \"{mangoTitle}\"");

                return true;
            }
            catch (PageFetchException ex) when (ex is PageFetchNotFoundException && groupScrapping)
            {
                Log.Error(ex.Message);
                return true;
            }
            catch (PageFetchException ex)
            {
                Log.Error(ex.Message);
                return false;
            }
            finally
            {
                if (TokenSource.IsCancellationRequested && currentFolder != "" && Directory.Exists(currentFolder))
                {
                    Directory.Delete(currentFolder, true);
                }
            }
        }

        public async Task<bool> ScrapGroupChapters(string url, int skipMango, (bool skipChapters, decimal from, decimal to) chapterRange)
        {
            int skippedMangos = 0;
            try
            {
                skipMango = toSkipMango > skipMango ? toSkipMango : skipMango;
                toSkipMango = skipMango;
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, TokenSource.Token));
                string[] groupName = new string[]
                {
                    parser.ParseGroupName(doc)
                };
                var mangos = parser.ParseGroupMangos(doc);
                Log.Information($"Scrapping chapters by \"{groupName[0]}\"");

                foreach (string mangoUrl in mangos)
                {
                    if (skippedMangos++ < skipMango)
                    {
                        continue;
                    }
                    if (await ScrapBulkChapters(mangoUrl, groupName, chapterRange, true))
                    {
                        return false;
                    }

                    toSkipMango++;
                    TokenSource.Token.ThrowIfCancellationRequested();
                    Log.Information("Waiting 2 sec before next mango.");
                    await Task.Delay(2000);
                }

                Log.Information($"Done scrapping chapters by \"{groupName[0]}\"");

                return true;
            }
            catch (PageFetchException ex)
            {
                //log
                return false;
            }
        }

        private ResiliencePipeline SetRetryPipeline()
        {
            var retryOptions = new Polly.Retry.RetryStrategyOptions()
            {
                ShouldHandle = new PredicateBuilder().Handle<PageFetchFailureException>().Handle<PageFetchRateLimitedException>(),
                BackoffType = DelayBackoffType.Constant,
                UseJitter = true,
                MaxRetryAttempts = Settings.Default.MaxRetries,
                Delay = TimeSpan.FromMilliseconds(Settings.Default.RetryDelay),
                OnRetry = static args =>
                {
                    Log.Error(args.Outcome.Exception.Message);
                    Log.Error($"Retrying ... (attempt {args.AttemptNumber} out of {Settings.Default.MaxRetries}");
                    return default;
                }
            };
            return new ResiliencePipelineBuilder().AddRetry(retryOptions).Build();
        }

    }
}
