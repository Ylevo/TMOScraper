using Newtonsoft.Json.Linq;
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
        private readonly string domainName = Settings.Default.Domain;
        private readonly HtmlDocument doc;
        private ResiliencePipeline retryPipeline;
        private int toSkipMango = 0;
        private readonly string language;
        private const string FolderNameTemplate = "{0} [{1}] - {2} [{3}]";


        public Scrapper(CancellationTokenSource tokenSource, HtmlDocument document) 
        {
            TokenSource = tokenSource;
            doc = document;
            language = Settings.Default.Language;
            retryPipeline = SetRetryPipeline();
        }

        public async Task<ScrappingResult> ScrapChapters(
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
                    return ScrappingResult.PageNotFound;
            }
        }

        public async Task<(ScrappingResult result, List<string>? groups)> ScrapScanGroups(string url)
        {
            try
            {
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, TokenSource.Token));
            }
            catch(UriFormatException)
            {
                return (ScrappingResult.PageNotFound, null);
            }
            catch(PageFetchException ex)
            {
                Log.Error(ex.Message);
                return (ScrappingResult.ImplementationFailure, null);
            }

            List<string> scanGroups = HtmlParser.ParseScanGroups(doc);

            if (scanGroups == null || scanGroups.Count == 0)
            {
                return (ScrappingResult.PageNotFound, null);
            }

            scanGroups.Sort();

            return (ScrappingResult.Success, scanGroups);
        }

        public async Task<ScrappingResult> ScrapSingleChapter(string url)
        {
            string mangoTitle,
                chapterNumber,
                groupName,
                currentFolder = "",
                mainFolder = Settings.Default.MainFolder;
            List<string> imgUrls;

            try
            {
                Log.Information("Scrapping single chapter.");
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token, PageType.Chapter); }, TokenSource.Token));

                chapterNumber = HtmlParser.ParseChapterNumberFromChapterPage(doc);
                groupName = HtmlParser.ParseGroupNameFromChapterPage(doc);
                mangoTitle = HtmlParser.ParseMangoTitleFromChapterPage(doc);
                imgUrls = HtmlParser.ParseChapterImages(doc);

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

                    Log.Information($"Downloading chapter {chapterNumber} by \"{groupName}\"");
                    await Downloader.DownloadChapter(currentFolder, imgUrls, TokenSource.Token);
                    Log.Information($"Done downloading chapter {chapterNumber} by \"{groupName}\"");

                    if (Settings.Default.ConvertImages)
                    {
                        await ImageUtil.ConvertImages(currentFolder, TokenSource.Token);
                    }

                    if (Settings.Default.SplitImages)
                    {
                        await ImageUtil.SplitImages(currentFolder, TokenSource.Token);
                    }
                }
                return ScrappingResult.Success;
            }
            catch (PageFetchException ex)
            {
                Log.Error(ex.Message);
                return ScrappingResult.ImplementationFailure;
            }
            finally
            {
                if (TokenSource.IsCancellationRequested && currentFolder != "" && Directory.Exists(currentFolder))
                {
                    Directory.Delete(currentFolder, true);
                }
            }
        }

        public async Task<ScrappingResult> ScrapBulkChapters(string url, string[]? groups, (bool skipChapters, decimal from, decimal to) chapterRange, bool groupScrapping = false)
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
                if (groups == null)
                {
                    return ScrappingResult.NoGroupSelected;
                }

                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, TokenSource.Token));
                mangoTitle = HtmlParser.ParseMangoTitleFromMangoPage(doc);

                Log.Information($"Scrapping chapters of \"{mangoTitle}\"");

                SortedDictionary<string, (string groupName, string chapterLink)[]> chapters = isOneShot ? HtmlParser.ParseOneShotLinks(doc) : HtmlParser.ParseChaptersLinks(doc);

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
                    TokenSource.Token.ThrowIfCancellationRequested();

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
                            imgUrls = HtmlParser.ParseChapterImages(doc);
                            Directory.CreateDirectory(currentFolder);

                            Log.Information($"Downloading chapter {chapterNumber} by \"{groupName}\"");
                            await Downloader.DownloadChapter(currentFolder, imgUrls, TokenSource.Token);
                            Log.Information($"Done downloading chapter {chapterNumber} by \"{groupName}\"");

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
                        Log.Information("No upload found or chapter already downloaded.");
                    }
                }

                Log.Information($"Done scrapping chapters of \"{mangoTitle}\"");

                return ScrappingResult.Success;
            }
            catch (PageFetchException ex) when (ex is PageFetchNotFoundException && groupScrapping)
            {
                Log.Warning(ex.Message);
                return ScrappingResult.PageNotFound;
            }
            catch (PageFetchException ex)
            {
                Log.Error($"Max retries reached : {ex.Message}");
                return ScrappingResult.ImplementationFailure;
            }
            finally
            {
                if (TokenSource.IsCancellationRequested && currentFolder != "" && Directory.Exists(currentFolder))
                {
                    Directory.Delete(currentFolder, true);
                }
            }
        }

        public async Task<ScrappingResult> ScrapGroupChapters(string url, int skipMango, (bool skipChapters, decimal from, decimal to) chapterRange)
        {
            int skippedMangos = 0;
            List<string> notFoundMangos = new();
            try
            {
                skipMango = toSkipMango > skipMango ? toSkipMango : skipMango;
                toSkipMango = skipMango;
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, TokenSource.Token));
                string[] groupName = new string[]
                {
                    HtmlParser.ParseGroupName(doc)
                };
                var mangos = HtmlParser.ParseGroupMangos(doc);
                Log.Information($"Scrapping chapters by \"{groupName[0]}\"");

                foreach (string mangoUrl in mangos)
                {
                    if (skippedMangos++ < skipMango)
                    {
                        continue;
                    }

                    ScrappingResult result = await ScrapBulkChapters(mangoUrl, groupName, chapterRange, true);

                    switch(result)
                    {
                        case ScrappingResult.Success:
                            break;
                        case ScrappingResult.PageNotFound:
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

                Log.Information($"Done scrapping chapters by \"{groupName[0]}\"");

                return ScrappingResult.Success;
            }
            catch (PageFetchException ex)
            {
                Log.Error($"Max retries reached, couldn't fetch the group page : {ex.Message}");
                return ScrappingResult.ImplementationFailure;
            }
            finally
            {
                if (notFoundMangos.Count > 0)
                {
                    Log.Warning($"{notFoundMangos.Count} mango were not found : \n{string.Join("\n", notFoundMangos.ToArray())}");
                }
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
                    Log.Error($"Retrying ... (attempt {args.AttemptNumber+1} out of {Settings.Default.MaxRetries})");
                    return default;
                }
            };
            return new ResiliencePipelineBuilder().AddRetry(retryOptions).Build();
        }

    }
}
