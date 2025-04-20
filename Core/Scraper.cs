using Polly;
using Serilog;
using System.Text.RegularExpressions;
using TMOScraper.Core.PageFetcher;
using TMOScraper.Properties;
using TMOScraper.Utils;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace TMOScraper.Core
{
    public partial class Scraper
    {
        public IPageFetcher? PageFetcher { get; set; } = null;
        public CancellationTokenSource? TokenSource { get; init; }
        private readonly string domainName = Settings.Default.Domain;
        private readonly HtmlDocument doc;
        private ResiliencePipeline retryPipeline;
        private int toSkipMango = 0;
        private readonly string language;
        private const string FolderNameTemplate = "{0} [{1}] - {2} ({3}) [{4}]";


        public Scraper(CancellationTokenSource tokenSource, HtmlDocument document) 
        {
            TokenSource = tokenSource;
            doc = document;
            language = Settings.Default.Language;
            retryPipeline = SetRetryPipeline();
        }

        public async Task<ScrapingResult> ScrapChapters(
            string url, 
            string[]? groups, 
            (bool skipChapters, decimal from, decimal to) chapterRange,
            int skipMango,
            bool allGroups)
        {
            TokenSource.Token.ThrowIfCancellationRequested();
            toSkipMango = toSkipMango < skipMango ? skipMango : toSkipMango;
            string pattern = $@"(?<={domainName}\/)((?<Bulk>library\/(manga|manhua|manhwa|doujinshi|one_shot|novel|oel)\/)|(?<Single>view_uploads|viewer\/)|(?<Group>groups\/(.*)proyects))";
            string pageType = Regex.Match(url, pattern, RegexOptions.ExplicitCapture).Groups.Values.Where(g => g.Success && g.Name != "0").FirstOrDefault()?.Name ?? "";
            switch (pageType)
            {
                case "Bulk":
                    return await ScrapBulkChapters(url, groups, chapterRange, allGroups: allGroups);
                case "Single":
                    return await ScrapSingleChapter(url);
                case "Group":
                    return await ScrapGroupChapters(url, skipMango, chapterRange);
                default:
                    return ScrapingResult.PageNotFound;
            }
        }

        public async Task<(ScrapingResult result, List<string>? groups)> ScrapScanGroups(string url)
        {
            Log.Information("Scraping scannies.");

            try
            {
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, TokenSource.Token));
            }
            catch(UriFormatException)
            {
                return (ScrapingResult.PageNotFound, null);
            }
            catch(PageFetchException ex)
            {
                Log.Error(ex.Message);
                return (ScrapingResult.PageFetchingFailure, null);
            }

            List<string>? scanGroups = HtmlQueries.GetScanGroups(doc);

            if (scanGroups == null || scanGroups.Count == 0)
            {
                return (ScrapingResult.PageNotFound, null);
            }

            scanGroups.Sort();

            return (ScrapingResult.Success, scanGroups);
        }

        public async Task<ScrapingResult> ScrapSingleChapter(string url)
        {
            string mangoTitle,
                chapterNumber,
                chapterTitle,
                groupName,
                currentFolder = "",
                mainFolder = Settings.Default.MainFolder;
            List<string> imgUrls;

            try
            {
                Log.Information("Started single chapter scraping.");
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token, PageType.Chapter); }, TokenSource.Token));

                chapterNumber = HtmlQueries.GetChapterNumberFromChapterPage(doc);
                chapterTitle = Settings.Default.ScrapChapterTitles ? HtmlQueries.GetChapterTitleFromChapterPage(doc) : "";
                groupName = HtmlQueries.GetGroupNameFromChapterPage(doc);
                mangoTitle = HtmlQueries.GetMangoTitleFromChapterPage(doc);
                imgUrls = HtmlQueries.GetChapterImages(doc);

                if (Settings.Default.SubFolder)
                {
                    mainFolder = Path.Combine(mainFolder, mangoTitle);
                    Directory.CreateDirectory(mainFolder);
                }

                currentFolder = Path.Combine(mainFolder, String.Format(FolderNameTemplate, mangoTitle, language, chapterNumber, chapterTitle, groupName));

                if (FolderExists(currentFolder))
                {
                    Log.Warning($"Skipping chapter {chapterNumber} by \"{groupName}\". Folder already exists.");
                }
                else
                {
                    Directory.CreateDirectory(currentFolder);

                    Log.Information($"Downloading chapter {chapterNumber} by \"{groupName}\".");
                    await Downloader.DownloadChapter(currentFolder, imgUrls, TokenSource.Token);
                    Log.Information($"Done downloading chapter {chapterNumber} by \"{groupName}\".");

                    if (Settings.Default.ConvertImages)
                    {
                        await ImageUtil.ConvertImages(currentFolder, TokenSource.Token);
                    }

                    if (Settings.Default.SplitImages)
                    {
                        await ImageUtil.SplitImages(currentFolder, TokenSource.Token);
                    }
                }
                return ScrapingResult.Success;
            }
            catch (PageFetchException ex)
            {
                Log.Error(ex.Message);
                return ScrapingResult.PageFetchingFailure;
            }
            finally
            {
                if (TokenSource.IsCancellationRequested && currentFolder != "" && Directory.Exists(currentFolder))
                {
                    Directory.Delete(currentFolder, true);
                }
            }
        }

        public async Task<ScrapingResult> ScrapBulkChapters(string url, string[]? groups, (bool skipChapters, decimal from, decimal to) chapterRange, bool groupScraping = false, bool allGroups = false)
        {
            string mangoTitle = "",
               chapterNumber,
               currentFolder = "",
               mainFolder = Settings.Default.MainFolder;
            bool actuallyDidSomething = false,
                isOneShot = url.Contains("/one_shot/");
            List<string> imgUrls;

            try
            {
                if (!groupScraping) { Log.Information($"Started mango scraping."); }
                if (groups == null && !allGroups){ return ScrapingResult.NoGroupSelected; }

                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, TokenSource.Token));
                mangoTitle = HtmlQueries.GetMangoTitleFromMangoPage(doc);

                Log.Information($"Scraping chapters of \"{mangoTitle}\".");

                SortedDictionary<string, (string groupName, string chapterLink, string chapterTitle)[]> chapters = isOneShot ? HtmlQueries.GetOneShotLinks(doc) : HtmlQueries.GetChaptersLinks(doc);

                if (chapterRange.skipChapters && !isOneShot)
                {

                    chapters = new SortedDictionary<string, (string groupName, string chapterLink, string chapterTitle)[]>
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
                    TokenSource.Token.ThrowIfCancellationRequested();

                    actuallyDidSomething = false;
                    chapterNumber = isOneShot ? "000" : "c" + chapter.Key;
                    Log.Information($"Scraping chapter {chapterNumber} of \"{mangoTitle}\".");

                    foreach (var (groupName, chapterLink, chapterTitle) in chapter.Value)
                    {
                        currentFolder = "";

                        TokenSource.Token.ThrowIfCancellationRequested();

                        if (allGroups || groups.Contains(groupName) || (groupScraping && groups.Any(groupName.Contains)))
                        {
                            currentFolder = Path.Combine(mainFolder, String.Format(FolderNameTemplate, mangoTitle, language, chapterNumber, Settings.Default.ScrapChapterTitles ? chapterTitle : "", groupName));

                            var foldersPresent = Directory.GetDirectories(mainFolder).Select(d => new DirectoryInfo(d).Name);

                            if (FolderExists(currentFolder))
                            {
                                Log.Warning($"Skipping chapter {chapterNumber} by \"{groupName}\". Folder already exists.");
                                continue;
                            }

                            doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(chapterLink, token, PageType.Chapter); }, TokenSource.Token));
                            imgUrls = HtmlQueries.GetChapterImages(doc);
                            Directory.CreateDirectory(currentFolder);

                            Log.Information($"Downloading chapter {chapterNumber} by \"{groupName}\".");
                            await Downloader.DownloadChapter(currentFolder, imgUrls, TokenSource.Token);
                            Log.Information($"Done downloading chapter {chapterNumber} by \"{groupName}\".");

                            if (Settings.Default.ConvertImages)
                            {
                                await ImageUtil.ConvertImages(currentFolder, TokenSource.Token);
                            }

                            if (Settings.Default.SplitImages) 
                            {
                                await ImageUtil.SplitImages(currentFolder, TokenSource.Token);
                            }

                            Log.Information($"Waiting {Settings.Default.ChapterDelay} ms before next chapter ...");
                            await Task.Delay(Settings.Default.ChapterDelay);

                            actuallyDidSomething = true;
                        }
                    }

                    if (!actuallyDidSomething)
                    {
                        Log.Information("No upload found or chapter(s) already downloaded.");
                    }
                    else
                    {
                        Log.Information($"Done scraping chapter {chapterNumber} of \"{mangoTitle}\".");
                    }
                }

                Log.Information($"Done scraping chapters of \"{mangoTitle}\".");

                return ScrapingResult.Success;
            }
            catch (PageFetchException ex) when (ex is PageFetchNotFoundException && (groupScraping || allGroups))
            {
                Log.Warning(ex.Message);
                return ScrapingResult.PageNotFound;
            }
            catch (PageFetchException ex)
            {
                Log.Error(ex.Message);
                return ScrapingResult.PageFetchingFailure;
            }
            finally
            {
                if (TokenSource.IsCancellationRequested && currentFolder != "" && Directory.Exists(currentFolder))
                {
                    Directory.Delete(currentFolder, true);
                }
            }
        }

        public async Task<ScrapingResult> ScrapGroupChapters(string url, int skipMango, (bool skipChapters, decimal from, decimal to) chapterRange)
        {
            int skippedMangos = 0;
            List<string> notFoundMangos = new();
            try
            {
                Log.Information($"Started group scraping.");
                skipMango = toSkipMango > skipMango ? toSkipMango : skipMango;
                toSkipMango = skipMango;
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, TokenSource.Token));
                string[] groupName = new string[]
                {
                    HtmlQueries.GetGroupName(doc)
                };
                var mangos = HtmlQueries.GetGroupMangos(doc);
                Log.Information($"Scraping chapters by \"{groupName[0]}\".");

                foreach (string mangoUrl in mangos)
                {
                    if (skippedMangos++ < skipMango)
                    {
                        continue;
                    }

                    ScrapingResult result = await ScrapBulkChapters(mangoUrl, groupName, chapterRange, true);

                    switch(result)
                    {
                        case ScrapingResult.Success:
                            break;
                        case ScrapingResult.PageNotFound:
                            notFoundMangos.Add(mangoUrl);
                            break;
                        default:
                            return result;
                    }

                    toSkipMango++;
                    TokenSource.Token.ThrowIfCancellationRequested();
                    Log.Information($"Waiting {Settings.Default.MangoDelay} ms before next mango ...");
                    await Task.Delay(Settings.Default.MangoDelay);
                }

                Log.Information($"Done scraping chapters by \"{groupName[0]}\".");

                return ScrapingResult.Success;
            }
            catch (PageFetchException ex)
            {
                Log.Error(ex.Message);
                return ScrapingResult.PageFetchingFailure;
            }
            finally
            {
                if (notFoundMangos.Count > 0)
                {
                    Log.Warning($"{notFoundMangos.Count} mango were not found : \n{string.Join("\n", notFoundMangos.ToArray())}");
                }
            }
        }

        private bool FolderExists(string path)
        {
            string mainFolder = Directory.GetParent(path).FullName;
            string folderToLookFor = Path.GetFileName(path);
            var foldersPresent = Directory.GetDirectories(mainFolder).Select(d => new DirectoryInfo(d).Name).ToList();

            for (int i = 0; i < foldersPresent.Count; i++)
            {
                Match m = Filename_Regex().Match(foldersPresent[i]);
                foldersPresent[i] = m.Groups["title"] + " [" + m.Groups["language"] + "] - " + m.Groups["prefix"] + m.Groups["chapter"] + " [" + m.Groups["group"] + "]";
            }

            return Directory.Exists(path) || foldersPresent.Contains(folderToLookFor);
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
                    Log.Error($"Retrying ... (attempt {args.AttemptNumber+1} out of {Settings.Default.MaxRetries})");
                    return default;
                }
            };
            return new ResiliencePipelineBuilder().AddRetry(retryOptions).Build();
        }

        [GeneratedRegex(@"(?:\[(?<artist>.+?)?\])?\s?(?<title>.+?)(?:\s?\[(?<language>[a-z]{2}(?:-[a-z]{2})?|[a-zA-Z]{3}|[a-zA-Z]+)?\])?\s-\s(?<prefix>(?:[c](?:h(?:a?p?(?:ter)?)?)?\.?\s?))?(?<chapter>\d+(?:\.\d+)?)(?:\s?\((?:[v](?:ol(?:ume)?(?:s)?)?\.?\s?)?(?<volume>\d+(?:\.\d+)?)?\))?(?:\s?\((?<chapter_title>.+)?\))?(?:\s?\{(?<publish_date>(?<publish_year>\d{4})-(?<publish_month>\d{2})-(?<publish_day>\d{2})(?:[T\s](?<publish_hour>\d{2})[\:\-](?<publish_minute>\d{2})(?:[\:\-](?<publish_microsecond>\d{2}))?(?:(?<publish_offset>[+-])(?<publish_timezone>\d{2}[\:\-]?\d{2}))?)?)\})?(?:\s?\[(?:(?<group>.+))?\])?(?:\s?\{v?(?<version>\d)?\})?(?:\.(?<extension>zip|cbz))?$", RegexOptions.IgnoreCase, "fr-FR")]
        private static partial Regex Filename_Regex();
    }
}
