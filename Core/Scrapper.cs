﻿using HtmlAgilityPack;
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
using System.Threading;
using TMOScrapper.Utils;
using System.Net;
using Serilog;

namespace TMOScrapper.Core
{
    internal class Scrapper
    {
        public IPageFetcher? PageFetcher { get; set; } = null;
        public CancellationTokenSource? CancellationTokenSource { get; set; } = null;
        private HtmlParser parser;
        private readonly string domainName = Settings.Default.Domain;
        private readonly HtmlDocument doc;
        private ResiliencePipeline retryPipeline;
        private int toSkipMango = 0;
        private string mainFolder;
        private readonly string language;
        private const string FolderNameTemplate = "{0} [{1}] - {2} [{3}]";


        public Scrapper(HtmlDocument document, HtmlParser htmlParser) 
        {
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
            string pattern = $@"(?<={domainName}\/)
                               ((?<Bulk>library\/(manga|manhua|manhwa|doujinshi|one_shot)\/)
                               |(?<Single>view_uploads|viewer\/)
                               |(?<Group>groups\/(.*)proyects))";
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

        public async Task<(bool result, List<string> groups)> ScrapScanGroups(string url)
        {
            List<string> scanGroups = new();

            try
            {
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, CancellationTokenSource.Token));
            }
            catch(PageFetchException ex)
            {
                //log
                return (false, scanGroups);
            }

            var scanGroupsNodes = parser.ParseScanGroups(doc);

            if (scanGroupsNodes == null)
            {
                Log.Error("404 scannies not found. Check your URL.");
                return (false, scanGroups);
            }

            foreach (var scanGroupNode in scanGroupsNodes)
            {
                scanGroups.Add(String.Join('+', scanGroupNode.ParentNode.InnerText.Split(',', StringSplitOptions.TrimEntries)));
            }

            scanGroups = scanGroups.Distinct().ToList();
            scanGroups.Sort();

            return (true, scanGroups);
        }

        public async Task<bool> ScrapSingleChapter(string url)
        {
            string mangaTitle,
                chapterNumber,
                groupName,
                currentFolder = "";
            HtmlNodeCollection imgNodes;

            try
            {
                //AddLog("Downloading single chapter.");
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token, PageType.Chapter); }, CancellationTokenSource.Token));

                var headerWithChapNumberAndGroups = doc.DocumentNode.SelectSingleNode("//h2");
                chapterNumber = doc.DocumentNode.SelectSingleNode("//h4").InnerText.Contains("ONE SHOT") ? "000"
                                : "c" + parser.ParseAndPadChapterNumber(headerWithChapNumberAndGroups.InnerText.Substring(9).Trim());
                groupName = String.Join('+', headerWithChapNumberAndGroups.Elements("a").Select(d => d.InnerText).ToArray());
                mangaTitle = parser.CleanMangoTitle(doc.DocumentNode.SelectSingleNode("//h1").InnerText);
                imgNodes = parser.ParseChapterImages(doc);

                if (Settings.Default.SubFolder)
                {
                    mainFolder = Path.Combine(mainFolder, mangaTitle);
                    Directory.CreateDirectory(mainFolder);
                }

                currentFolder = Path.Combine(mainFolder, String.Format(FolderNameTemplate, mangaTitle, language, chapterNumber, groupName));

                if (Directory.Exists(currentFolder))
                {
                    //AddLog("Skipping chapter " + chapterNumber + " by '" + groupName + "'. Folder already exists.");
                }
                else
                {
                    Directory.CreateDirectory(currentFolder);

                    //AddLog("Downloading chapter " + chapterNumber + " by '" + groupName + "'");
                    await Downloader.DownloadChapter(currentFolder, imgNodes, CancellationTokenSource.Token);
                    //AddLog("Done downloading chapter " + chapterNumber + " by '" + groupName + "'");
                }
                return true;
            }
            catch (PageFetchException ex)
            {
                //log
                return false;
            }
            finally
            {
                if (CancellationTokenSource.IsCancellationRequested && currentFolder != "")
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
            HtmlNodeCollection imgNodes;

            try
            {
                if (groups == null)
                {
                    Log.Error("No scannies selected, aborting scrapping.");
                    return true;
                }

                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, CancellationTokenSource.Token));
                mangoTitle = parser.CleanMangoTitle(parser.ParseMangoTitleFromMangoPage(doc));

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

                        CancellationTokenSource.Token.ThrowIfCancellationRequested();

                        if (groups.Contains(groupName) || (groupScrapping && groups.Any(groupName.Contains)))
                        {
                            currentFolder = Path.Combine(mainFolder, String.Format(FolderNameTemplate, mangoTitle, language, chapterNumber, groupName));

                            if (Directory.Exists(currentFolder))
                            {
                                Log.Warning($"Skipping chapter {chapterNumber} by \"{groupName}\". Folder already exists.");
                                continue;
                            }

                            doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(chapterLink, token, PageType.Chapter); }, CancellationTokenSource.Token));
                            imgNodes = parser.ParseChapterImages(doc);
                            Directory.CreateDirectory(currentFolder);

                            Log.Information($"Download chapter {chapterNumber} by \"{groupName}\"");
                            await Downloader.DownloadChapter(currentFolder, imgNodes, CancellationTokenSource.Token);
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
                return true;
            }
            catch (PageFetchException ex)
            {
                Log.Error(ex.Message);
                return false;
            }
            finally
            {
                if (CancellationTokenSource.IsCancellationRequested && currentFolder != "")
                {
                    Directory.Delete(currentFolder, true);
                }
            }
        }

        public async Task<bool> ScrapGroupChapters(string url, int skipMango, (bool skipChapters, decimal from, decimal to) chapterRange)
        {
            string mangoUrl;
            int skippedMangos = 0;
            try
            {
                skipMango = toSkipMango > skipMango ? toSkipMango : skipMango;
                toSkipMango = skipMango;
                doc.LoadHtml(await retryPipeline.ExecuteAsync(async token => { return await PageFetcher.GetPage(url, token); }, CancellationTokenSource.Token));
                string[] groupName = new string[]
                {
                    parser.ParseGroupName(doc)
                };
                var mangos = parser.ParseGroupMangos(doc);
                Log.Information($"Scrapping chapters by \"{groupName[0]}\"");

                foreach (var mango in mangos)
                {
                    if (skippedMangos++ < skipMango)
                    {
                        continue;
                    }
                    mangoUrl = mango.Attributes["href"].Value;
                    if (await ScrapBulkChapters(url, groupName, chapterRange, true))
                    {
                        return false;
                    }

                    toSkipMango++;
                    CancellationTokenSource.Token.ThrowIfCancellationRequested();
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
