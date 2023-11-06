using HtmlAgilityPack;
using PuppeteerSharp;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.XPath;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace SpanishScraper
{
    public partial class Form1 : Form
    {
        private readonly HttpClient httpClient;
        private readonly HtmlWeb webClient;
        private IBrowser? browser;
        private IResponse? lastResponse;
        private readonly NavigationOptions navigationOptionsDefault = new() { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.DOMContentLoaded }, Timeout = 6000 };
        private readonly NavigationOptions navigationOptionsRedirect = new() { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle2 }, Timeout = 5000 };
        private HtmlDocument doc = new();
        private readonly Random random = new();
        private CancellationTokenSource cancellationToken = new();
        private int waitingTimeBetweenChapters = 3000;
        private const string domainName = "https://visortmo.com";
        private const string folderNameTemplate = "{0} [{1}] - {2} [{3}]";
        private readonly Dictionary<string, string> langDict = new()
        {
            {"Spanish ","es" },
            {"Spanish (LATAM) ","es-la" }
        };
        public Form1()
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;

            InitializeComponent();
            AddLog("Downloading Chromium ...");
            var progress = new Progress<bool>(value =>
            {
                AddLog("Done downloading Chromium.");
                btn_download.Enabled = value;
                btn_scan.Enabled = value;
            });
            Task.Run(async () => await InitializePuppeteer(progress));

            languageCmbBox.DataSource = new BindingSource(langDict, null);
            languageCmbBox.DisplayMember = "Key";
            languageCmbBox.ValueMember = "Value";
            txtBox_setFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            httpClient = new HttpClient(new SocketsHttpHandler() { MaxConnectionsPerServer = 5 });
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:108.0) Gecko/20100101 Firefox/108.0");
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.Add("Referer", domainName);

            webClient = new HtmlWeb();
            webClient.PreRequest += delegate (HttpWebRequest req)
            {
                req.Referer = domainName;
                return true;
            };
        }

        private async Task InitializePuppeteer(IProgress<bool> progress)
        {
            await new BrowserFetcher().DownloadAsync();
            browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            progress.Report(true);
        }

        private void Btn_setFolder_Click(object sender, EventArgs e)
        {
            if (setFolderDialog.ShowDialog() == DialogResult.OK)
            {
                txtBox_setFolder.Text = setFolderDialog.SelectedPath;
            }
        }

        private async void Btn_scan_Click(object sender, EventArgs e)
        {
            if (txtBox_mangoUrl.Text == String.Empty || txtBox_setFolder.Text == string.Empty)
            {
                MessageBox.Show("Retard.", "Retard Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Hand);
                return;
            }
            await GetPage(txtBox_mangoUrl.Text);
            ListScantardsGroups(doc);
        }
        
        private async void Btn_download_Click(object sender, EventArgs e)
        {
            cancellationToken = new CancellationTokenSource();
            listbox_logger.Items.Clear();
            ToggleButtonsAndShit();

            switch (txtBox_mangoUrl.Text)
            {
                case string url when url.Contains(domainName + "/library/manga/")
                                  || url.Contains(domainName + "/library/one_shot/"):
                    await BulkChaptersDownload();
                    break;
                case string url when url.Contains(domainName + "/view_uploads/")
                                  || url.Contains(domainName + "/viewer/"):
                    await SingleChapterDownload();
                    break;
                case string url when url.Contains(domainName + "/groups/") && url.Contains("/proyects"):
                    await GroupChaptersDownload();
                    break;
                default:
                    AddLog("Error: wrong URL.");
                    break;
            }

            ToggleButtonsAndShit();
        }

        private async Task GroupChaptersDownload()
        {
            string mangoUrl, mangoTitle;
            int skippedMangos = 0,
                mangosToSkip = checkBox_skipMangos.Checked ? (int)numeric_skipMangos.Value : -1;
            try
            {
                await GetPage(txtBox_mangoUrl.Text);
                string[] groupName = new string[]
                { 
                    doc.DocumentNode.SelectSingleNode("//h1").InnerText.Trim() 
                };

                var mangos = doc.DocumentNode.SelectNodes("//div[contains(concat(' ',normalize-space(@class),' '),' proyect-item ')]/a");
                
                foreach(var mango in mangos)
                {
                    if (skippedMangos++ < mangosToSkip)
                    {
                        continue;
                    }
                    mangoTitle = mango.Descendants("h4").First().InnerText;
                    AddLog("Downloading chapters of " + mangoTitle);
                    mangoUrl = mango.Attributes["href"].Value;
                    await BulkChaptersDownload(groupName, mangoUrl, true);

                    cancellationToken.Token.ThrowIfCancellationRequested();
                    AddLog("Done with " + mangoTitle);
                    AddLog("Waiting 2000 ms before next mango.");
                    await Task.Delay(2000);
                    listbox_logger.Items.Clear();
                }

            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                AddLog("Aborted group download.");
            }
            catch (Exception exc)
            {
                AddLog(exc.Message);
                AddLog("Something went wrong.");
                cancellationToken.Cancel();
            }
        }

        private async Task SingleChapterDownload()
        {
            string mainFolder = txtBox_setFolder.Text,
                mangaTitle,
                language = languageCmbBox.SelectedValue.ToString(),
                chapterNumber,
                groupName,
                currentFolder = "";

            try
            {
                AddLog("Downloading single chapter.");
                HtmlNodeCollection imgNodes = await GetChapterImgNodes(txtBox_mangoUrl.Text);

                var headerWithChapNumberAndGroups = doc.DocumentNode.SelectSingleNode("//h2");
                chapterNumber = doc.DocumentNode.SelectSingleNode("//h4").InnerText.Contains("ONE SHOT") ? "000" 
                                : "c" + ParseAndPadChapterNumber(headerWithChapNumberAndGroups.InnerText.Substring(9).Trim());
                groupName = String.Join('+', headerWithChapNumberAndGroups.Elements("a").Select(d => d.InnerText).ToArray());
                mangaTitle = CleanMangoTitle(doc.DocumentNode.SelectSingleNode("//h1").InnerText);

                if (checkBox_MangoSubfolder.Checked)
                {
                    mainFolder = Path.Combine(mainFolder, mangaTitle);
                    Directory.CreateDirectory(mainFolder);
                }

                currentFolder = Path.Combine(mainFolder, String.Format(folderNameTemplate, mangaTitle, language, chapterNumber, groupName));

                if (Directory.Exists(currentFolder))
                {
                    AddLog("Skipping chapter " + chapterNumber + " by '" + groupName + "'. Folder already exists.");
                }
                else
                {
                    Directory.CreateDirectory(currentFolder);

                    AddLog("Downloading chapter " + chapterNumber + " by '" + groupName + "'");
                    await DownloadChapter(currentFolder, "", imgNodes);
                    AddLog("Done downloading chapter " + chapterNumber + " by '" + groupName + "'");
                }

            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                AddLog("Stopped download.");
            }
            catch (Exception exc)
            {
                AddLog(exc.Message);
                AddLog("Something went wrong.");
                cancellationToken.Cancel();
            }
            finally
            {
                if (cancellationToken.IsCancellationRequested && currentFolder != "")
                {
                    Directory.Delete(currentFolder, true);
                }
            }
        }

        private async Task BulkChaptersDownload(string[]? groups = null, string? mangoUrl = null, bool includeJointGroupsChapters = false)
        {
            if (!listBox_Scannies.Visible && groups == null)
            {
                AddLog("Error: scan the scannies first.");
                return;
            }
            string mainFolder = txtBox_setFolder.Text,
                mangaTitle = "",
                language = languageCmbBox.SelectedValue.ToString(),
                chapterNumber,
                currentFolder = "";
            decimal chapterRangeFrom = numeric_chaptersRangeFrom.Value,
                    chapterRangeTo = numeric_chaptersRangeTo.Value;
            groups ??= listBox_Scannies.CheckedItems.Cast<string>().ToArray();
            mangoUrl ??= txtBox_mangoUrl.Text;
            bool actuallyDidSomething = false,
                isOneShot = mangoUrl.Contains("/one_shot/");

            try
            {
                await GetPage(mangoUrl);
                mangaTitle = CleanMangoTitle(doc.DocumentNode.SelectSingleNode("//h2").InnerText);
                SortedDictionary<string, (string GroupName, string ChapterLink)[]> chapters = isOneShot ? GetOneShotLinks(doc) : GetChaptersLinks(doc);

                if (checkBox_chaptersRange.Checked && !isOneShot)
                {
                    if (chapterRangeFrom > chapterRangeTo)
                    {
                        AddLog("Error: invalid chapter range.");
                        return;
                    }

                    chapters = new SortedDictionary<string, (string GroupName, string ChapterLink)[]> 
                                (chapters.Where(d => decimal.Parse(d.Key) >= chapterRangeFrom && decimal.Parse(d.Key) <= chapterRangeTo)
                                .ToDictionary(d => d.Key, d => d.Value));
                }

                if (checkBox_MangoSubfolder.Checked)
                {
                    mainFolder = Path.Combine(mainFolder, mangaTitle);
                    Directory.CreateDirectory(mainFolder);
                }

                foreach (var chapter in chapters)
                {
                    chapterNumber = isOneShot ? "000" : "c" + chapter.Key;
                    AddLog("Checking chapter " + chapterNumber);

                    foreach (var uploadedChapter in chapter.Value)
                    {
                        currentFolder = "";
                        
                        cancellationToken.Token.ThrowIfCancellationRequested();

                        if (groups.Contains(uploadedChapter.GroupName) || (includeJointGroupsChapters && groups.Any(uploadedChapter.GroupName.Contains)))
                        {
                            currentFolder = Path.Combine(mainFolder, String.Format(folderNameTemplate, mangaTitle, language, chapterNumber, uploadedChapter.GroupName));

                            if (Directory.Exists(currentFolder))
                            {
                                AddLog("Skipping chapter " + chapterNumber + " by '" + uploadedChapter.GroupName + "'. Folder already exists.");
                                continue;
                            }

                            Directory.CreateDirectory(currentFolder);
                            AddLog("Downloading chapter " + chapterNumber + " by '" + uploadedChapter.GroupName + "'");
                            await DownloadChapter(currentFolder, uploadedChapter.ChapterLink);
                            AddLog("Done downloading chapter " + chapterNumber + " by '" + uploadedChapter.GroupName + "'");
                            actuallyDidSomething = true;
                        }
                    }

                    if (actuallyDidSomething)
                    {
                        AddLog("Waiting " + waitingTimeBetweenChapters + " ms ...");
                        await Task.Delay(waitingTimeBetweenChapters);
                    }
                    else
                    {
                        AddLog("No upload found or chapter already downloaded.");
                    }

                    actuallyDidSomething = false;
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                AddLog("Stopped download.");
            }
            catch (Exception exc)
            {
                AddLog(exc.Message);
                AddLog("Something went wrong.");
                cancellationToken.Cancel();
            }
            finally
            {
                if (cancellationToken.IsCancellationRequested && currentFolder != "")
                {
                    Directory.Delete(currentFolder, true);
                }
            }
        }
        private async Task DownloadChapter(string folderPath, string chapterLink = "", HtmlNodeCollection? imgNodes = null)
        {
            imgNodes ??= await GetChapterImgNodes(chapterLink);
            string url, filename;
            var tasks = new List<Task>();

            for (int i=0 ; i < imgNodes.Count; i++)
            {
                url = imgNodes[i].Attributes["data-src"].Value;
                filename = $"{i:D3}." + url.Split('.').Last();
                tasks.Add(DownloadFile(new Uri(url), Path.Combine(folderPath, filename), filename));
            }

            await Task.WhenAll(tasks);
        }

        private async Task<HtmlNodeCollection> GetChapterImgNodes(string chapterLink)
        {
            using (var page = await browser.NewPageAsync())
            {
                HtmlNodeCollection? imgNodes = null;
                await ConfigurePuppeteerPage(page);

                while (imgNodes == null)
                {
                    try
                    {
                        await GetChapterPage(chapterLink, page);
                        doc.LoadHtml(await page.GetContentAsync());
                        imgNodes = doc.DocumentNode.SelectNodes("//img[contains(concat(' ',normalize-space(@class),' '),' viewer-img ')]");
                    }
                    catch(HttpRequestException exc)
                    {
                        throw;
                    }
                    catch(Exception ex) when (ex is not TaskCanceledException && ex is not OperationCanceledException)
                    {
                        AddLog("Error: " + ex.Message);
                        AddLog("Fetching chapter page failed. Retrying ...");
                        await Task.Delay(random.Next(1000, 2000));
                    }
                }

                return imgNodes;
            }
        }

        private async Task GetChapterPage(string chapterLink, IPage page, int counter = 1)
        {
            bool goodToGo = false;
            cancellationToken.Token.ThrowIfCancellationRequested();
            try
            {
                await page.GoToAsync(chapterLink, navigationOptionsDefault);

                if (page.Url.Contains("/view_uploads/"))
                {
                    await page.WaitForNavigationAsync(navigationOptionsRedirect);
                }
            }
            catch(NavigationException){ }

            string titleText = (await (await (await page.QuerySelectorAsync("title")).GetPropertyAsync("innerText")).JsonValueAsync()).ToString();

            switch(true)
            {
                case true when titleText.Contains("Estas haciendo demasiadas peticiones"):
                    AddLog($"Ratelimit hit. Waiting around 3-5 seconds ... ({counter})");
                    AddLog("Try increasing the delay if you get this often.");
                    await Task.Delay(random.Next(3000, 5000));
                    goto CaseUrl;

                case true when lastResponse.Status == HttpStatusCode.NotFound:
                    throw new HttpRequestException("Error: 404 page not found. Aborting.");

                case true when !lastResponse.Ok:
                    AddLog("Last chapter request failed.");
                    AddLog("Status code : " + lastResponse.Status);
                    AddLog($"Retrying ... ({counter})");
                    await Task.Delay(random.Next(1000, 2000));
                    break;

                case true when !page.Url.Contains(domainName + "/viewer/") || !page.Url.Contains("/cascade"):
                CaseUrl:

                    if (page.Url.Contains(domainName) || page.Url.Contains("/news/"))
                    {
                        chapterLink = page.Url;
                    }
                    else
                    {
                        AddLog($"Failed getting chapter page. Retrying ... ({counter})");
                    }

                    chapterLink = Regex.Replace(chapterLink, "https.+/news/", domainName + "/viewer/");
                    chapterLink = chapterLink.Replace("paginated", "cascade");
                    await Task.Delay(500);
                    break;

                default: 
                    goodToGo = true;
                    break;
            }

            if (!goodToGo)
            {
                await GetChapterPage(chapterLink, page, ++counter);
            }
        }

        private async Task GetPage(string url)
        {
            int counter = 1;
            string titleText;
            bool goodToGo = false;

            while (!goodToGo)
            {
                doc = webClient.Load(url);
                titleText = doc.DocumentNode.SelectSingleNode("//title").InnerHtml;

                if (titleText.Contains("Estas haciendo demasiadas peticiones"))
                {
                    AddLog($"Ratelimit hit. Waiting around 3-5 seconds ... ({counter++})");
                    await Task.Delay(random.Next(3000, 5000));
                }
                else
                {
                    goodToGo = true;
                }
            }
        }

        private async Task DownloadFile(Uri uri, string path, string filename)
        {
            cancellationToken.Token.ThrowIfCancellationRequested();

            using (var s = await httpClient.GetStreamAsync(uri, cancellationToken.Token))
            {
                using (var fs = new FileStream(path, FileMode.CreateNew))
                {
                    AddLog("Downloading file " + filename + " ...");
                    await s.CopyToAsync(fs, cancellationToken.Token);
                }
            }
        }

        private void ListScantardsGroups(HtmlDocument doc)
        {
            List<string> scanGroups = new();
            var scanGroupsNodes = doc.DocumentNode.SelectNodes(@"//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]
                                                                 //div[1][contains(concat(' ',normalize-space(@class),' '),' text-truncate ')]
                                                                 /span");
            if (scanGroupsNodes == null)
            {
                throw new Exception("Error: 404 scannies not found. Check your URL.");
            }

            foreach (var scanGroupNode in scanGroupsNodes)
            {
                scanGroups.Add(String.Join('+', scanGroupNode.ParentNode.InnerText.Split(',', StringSplitOptions.TrimEntries)));
            }

            scanGroups = scanGroups.Distinct().ToList();
            scanGroups.Sort();
            listBox_Scannies.Items.Clear();
            listBox_Scannies.Items.AddRange(scanGroups.ToArray());
            listBox_Scannies.Visible = true;
        }

        private SortedDictionary<string, (string, string)[]> GetChaptersLinks(HtmlDocument doc)
        {
            SortedDictionary<string, (string, string)[]> chapters = new();
            var chaptersNodes = doc.DocumentNode.SelectNodes("//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]");
            IEnumerable<HtmlNode> uploadedChaptersNodes;
            string chapterNumber, uploadedChapterLink, groupName;

            for (int i = 0; i < chaptersNodes.Count; ++i)
            {
                chapterNumber = ParseAndPadChapterNumber(chaptersNodes[i].Descendants("a").First().InnerText.Substring(9).Trim());

                uploadedChaptersNodes = chaptersNodes[i].Descendants("li");
                (string, string)[] uploadedChapters = new (string, string)[uploadedChaptersNodes.Count()];
                
                for (int x = 0; x < uploadedChaptersNodes.Count(); ++x)
                {
                    uploadedChapterLink = uploadedChaptersNodes.ElementAt(x).Descendants("a").Last().Attributes["href"].Value;
                    groupName = String.Join('+', uploadedChaptersNodes.ElementAt(x).Descendants("span").First().InnerText.Split(',', StringSplitOptions.TrimEntries));
                    uploadedChapters[x] = (groupName, uploadedChapterLink);
                }

                if (chapters.ContainsKey(chapterNumber))
                {
                    chapters[chapterNumber] = chapters[chapterNumber].Concat(uploadedChapters).ToArray();
                }
                else
                {
                    chapters.Add(chapterNumber, uploadedChapters);
                }
            }

            return chapters;
        }

        private SortedDictionary<string, (string, string)[]> GetOneShotLinks(HtmlDocument doc)
        {
            string uploadLink, groupName;
            SortedDictionary<string, (string, string)[]> chapter = new();
            var uploadNodes = doc.DocumentNode.SelectNodes("//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]");

            (string, string)[] uploadLinks = new (string, string)[uploadNodes.Count];

            for (int x = 0; x < uploadNodes.Count(); ++x)
            {
                uploadLink = uploadNodes.ElementAt(x).Descendants("a").Last().Attributes["href"].Value;
                groupName = String.Join('+', uploadNodes.ElementAt(x).Descendants("span").First().InnerText.Split(',', StringSplitOptions.TrimEntries));
                uploadLinks[x] = (groupName, uploadLink);
            }

            chapter.Add("000", uploadLinks);

            return chapter;
        }

        private string ParseAndPadChapterNumber(string chapterNumber)
        {
            chapterNumber = chapterNumber.Substring(0, chapterNumber.IndexOf('.') + 3);
            string split = chapterNumber.Split('.').Last();

            if (int.Parse(split) > 0)
            {
                if (split.Contains('0'))
                {
                    split = split.Replace("0", "");
                    chapterNumber = (chapterNumber.Remove(chapterNumber.IndexOf(".")+1) + split).PadLeft(5, '0');
                }
                else
                {
                    chapterNumber = chapterNumber.PadLeft(6, '0');
                }
            }
            else
            {
                chapterNumber = chapterNumber.Substring(0, chapterNumber.IndexOf(".")).PadLeft(3, '0');
            }

            return chapterNumber;
        }



        private async Task ConfigurePuppeteerPage(IPage page)
        {
            await page.SetExtraHttpHeadersAsync(new Dictionary<string, string> { { "Referer", domainName } });
            await page.SetJavaScriptEnabledAsync(true);
            await page.SetRequestInterceptionAsync(true);

            page.Response += (sender, e) =>
            {
                lastResponse = e.Response;
            };
            page.Request += (sender, e) =>
            {
                switch (e.Request.ResourceType)
                {
                    case ResourceType.Image:
                    case ResourceType.Img:
                    case ResourceType.StyleSheet:
                    case ResourceType.ImageSet:
                    case ResourceType.Media:
                        e.Request.AbortAsync();
                        break;
                    case ResourceType.Script:
                        if (!page.Url.Contains("/view_uploads/"))
                        {
                            e.Request.AbortAsync();
                        }
                        else
                        {
                            e.Request.ContinueAsync();
                        }
                        break;
                    default:
                        e.Request.ContinueAsync();
                        break;
                }
            };
        }

        private void ToggleButtonsAndShit()
        {
            btn_setFolder.Enabled = !btn_setFolder.Enabled;
            listBox_Scannies.Enabled = !listBox_Scannies.Enabled;
            btn_scan.Enabled = !btn_scan.Enabled;
            btn_download.Enabled = !btn_download.Enabled;
            btn_stop.Enabled = !btn_stop.Enabled;
        }

        private void AddLog(string message)
        {
            listbox_logger.Items.Insert(0, message);
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            cancellationToken?.Cancel();
            AddLog("Post-natal abortion requested.");
        }

        private string CleanMangoTitle(string filename)
        {
            return string.Join("", filename.Split(Path.GetInvalidPathChars())).Truncate(40).Trim().Replace(' ', '-');
        }

        private void txtBox_Delay_TextChanged(object sender, EventArgs e)
        {
            waitingTimeBetweenChapters = int.TryParse(txtBox_Delay.Text, out waitingTimeBetweenChapters) ? waitingTimeBetweenChapters : 3000;
        }

        private void txtBox_mangoUrl_TextChanged(object sender, EventArgs e)
        {
            listBox_Scannies.Visible = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                browser?.CloseAsync();
            }

            base.Dispose(disposing);
        }
    }

    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}