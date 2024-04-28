using HtmlAgilityPack;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerExtraSharp;
using PuppeteerSharp;
using System.Net;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.Integration;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using TMOScrapper.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System.Windows;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Serilog;
using Serilog.Sinks.RichTextBox.Themes;

namespace TMOScrapper
{
    public partial class MainForm : Form
    {
        private readonly IServiceProvider serviceProvider;
        private bool selectGroupsToggle = true;
        private readonly HttpClient httpClient;
        private readonly HtmlWeb webClient;
        private IBrowser? browser;
        private IResponse? lastResponse;
        private readonly NavigationOptions navigationOptionsDefault = new() { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.DOMContentLoaded }, Timeout = 6000 };
        private HtmlDocument doc = new();
        private readonly Random random = new();
        private CancellationTokenSource? cancellationToken = new();
        private int waitingTimeBetweenChapters = 3000;
        private const string DomainName = "https://visortmo.com";
        private const string FolderNameTemplate = "{0} [{1}] - {2} [{3}]";
        private readonly Dictionary<string, string> langDict = new()
        {
            {"Spanish ","es" },
            {"Spanish (LATAM) ","es-la" }
        };

        private static readonly object loggerSync = new object();
        private System.Windows.Controls.RichTextBox richTxtBoxLogger;

        public MainForm(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;

            InitializeComponent();
            AddRichTextBoxLogger();

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
            httpClient.DefaultRequestHeaders.Add("Referer", DomainName);

            webClient = new HtmlWeb();
            webClient.PreRequest += delegate (HttpWebRequest req)
            {
                req.Referer = DomainName;
                return true;
            };

            var t = GetNewScrapper();
        }

        private void AddRichTextBoxLogger()
        {
            var richTextBoxHost = new ElementHost
            {
                Dock = DockStyle.Fill,
            };
            panel_Logger.Controls.Add(richTextBoxHost);

            var wpfRichTextBox = new System.Windows.Controls.RichTextBox
            {
                Background = Brushes.White,
                Foreground = Brushes.Black,
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0),
            };

            richTextBoxHost.Child = wpfRichTextBox;
            richTxtBoxLogger = wpfRichTextBox;

            const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RichTextBox(wpfRichTextBox, theme: LoggerTheme.MyTheme, outputTemplate: outputTemplate, syncRoot: loggerSync)
                .CreateLogger();
        }

        private ScrapperHandler? GetNewScrapper()
        {
            var scrapper = serviceProvider.GetRequiredService<ScrapperHandler>();
            cancellationToken = scrapper?.CancellationTokenSource;

            return scrapper;
        }

        private async Task InitializePuppeteer(IProgress<bool> progress)
        {
            await new BrowserFetcher().DownloadAsync();
            var extra = new PuppeteerExtra();
            extra.Use(new StealthPlugin());
            browser = await extra.LaunchAsync(new LaunchOptions { Headless = true });
            progress.Report(true);
        }

        private void BtnSetFolder_Click(object sender, EventArgs e)
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
                //MessageBox.Show("URL and/or folder path is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                cancellationToken = new CancellationTokenSource();
                ToggleUI();

                await GetPage(txtBox_mangoUrl.Text);
                ListScanGroups(doc);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                AddLog("Cancelled scanning.");
            }
            catch (Exception exc)
            {
                AddLog(exc.Message);
                AddLog("Something went wrong.");
                cancellationToken.Cancel();
            }
            finally
            {
                ToggleUI();
            }
        }
        
        private async void BtnDownload_Click(object sender, EventArgs e)
        {
            cancellationToken = new CancellationTokenSource();
            //listbox_logger.Items.Clear();
            ToggleUI();
            string pattern =   $@"(?<={DomainName}\/)
                               ((?<Bulk>library\/(manga|manhua|manhwa|doujinshi|one_shot)\/)
                               |(?<Single>view_uploads|viewer\/)
                               |(?<Group>groups\/(.*)proyects))";
            string pageType = Regex.Match(txtBox_mangoUrl.Text, pattern, RegexOptions.ExplicitCapture).Groups.Values.Where(g => g.Success && g.Name != "0").FirstOrDefault()?.Name ?? "";

            switch (pageType)
            {
                case "Bulk":
                    await BulkChaptersDownload();
                    break;
                case "Single":
                    await SingleChapterDownload();
                    break;
                case "Group":
                    await GroupChaptersDownload();
                    break;
                default:
                    AddLog("Error: wrong URL.");
                    break;
            }

            ToggleUI();
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
                    try
                    {
                        await BulkChaptersDownload(groupName, mangoUrl, true, true);
                    }
                    catch(HttpRequestException httpEx)
                    {
                        if (httpEx.StatusCode == HttpStatusCode.NotFound)
                        {
                            AddLog("Mango URL not found for \"" + mangoTitle + "\". Continuing.");
                            continue;
                        }
                        throw;
                    }

                    cancellationToken.Token.ThrowIfCancellationRequested();
                    AddLog("Done with " + mangoTitle);
                    AddLog("Waiting 2000 ms before next mango.");
                    await Task.Delay(2000);
                    //listbox_logger.Items.Clear();
                }

                AddLog("Done downloading group mangos.");
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

                currentFolder = Path.Combine(mainFolder, String.Format(FolderNameTemplate, mangaTitle, language, chapterNumber, groupName));

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

        private async Task BulkChaptersDownload(string[]? groups = null, string? mangoUrl = null, bool includeJointGroupsChapters = false, bool continueIfMangoNotFound = false)
        {
            if (!listBox_Scannies.Visible && groups == null)
            {
                AddLog("Error: scan the scannies first.");
                return;
            }
            string mainFolder = txtBox_setFolder.Text,
                mangoTitle = "",
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
                mangoTitle = CleanMangoTitle(doc.DocumentNode.SelectSingleNode("//h2").InnerText);
                SortedDictionary<string, (string groupName, string chapterLink)[]> chapters = isOneShot ? GetOneShotLinks(doc) : GetChaptersLinks(doc);

                if (checkBox_chaptersRange.Checked && !isOneShot)
                {
                    if (chapterRangeFrom > chapterRangeTo)
                    {
                        AddLog("Error: invalid chapter range.");
                        return;
                    }

                    chapters = new SortedDictionary<string, (string groupName, string chapterLink)[]> 
                                (chapters.Where(d => decimal.Parse(d.Key) >= chapterRangeFrom && decimal.Parse(d.Key) <= chapterRangeTo)
                                .ToDictionary(d => d.Key, d => d.Value));
                }

                if (checkBox_MangoSubfolder.Checked)
                {
                    mainFolder = Path.Combine(mainFolder, mangoTitle);
                    Directory.CreateDirectory(mainFolder);
                }

                foreach (var chapter in chapters)
                {
                    actuallyDidSomething = false;
                    chapterNumber = isOneShot ? "000" : "c" + chapter.Key;
                    AddLog("Checking chapter " + chapterNumber);

                    foreach (var (groupName, chapterLink) in chapter.Value)
                    {
                        currentFolder = "";
                        
                        cancellationToken.Token.ThrowIfCancellationRequested();

                        if (groups.Contains(groupName) || (includeJointGroupsChapters && groups.Any(groupName.Contains)))
                        {
                            currentFolder = Path.Combine(mainFolder, String.Format(FolderNameTemplate, mangoTitle, language, chapterNumber, groupName));

                            if (Directory.Exists(currentFolder))
                            {
                                AddLog("Skipping chapter " + chapterNumber + " by '" + groupName + "'. Folder already exists.");
                                continue;
                            }

                            Directory.CreateDirectory(currentFolder);
                            AddLog("Downloading chapter " + chapterNumber + " by '" + groupName + "'");
                            await DownloadChapter(currentFolder, chapterLink);
                            AddLog("Done downloading chapter " + chapterNumber + " by '" + groupName + "'");
                            AddLog("Waiting " + waitingTimeBetweenChapters + " ms ...");
                            await Task.Delay(waitingTimeBetweenChapters);
                            actuallyDidSomething = true;
                        }
                    }

                    if (!actuallyDidSomething)
                    {
                        AddLog("No upload found or chapter already downloaded.");
                    }
                }

                AddLog("Done downloading chapters of " + mangoTitle);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                AddLog("Stopped download.");
            }
            catch(Exception ex) when (ex is HttpRequestException exception && continueIfMangoNotFound && exception.StatusCode == HttpStatusCode.NotFound)
            {
                throw;
            }
            catch (Exception ex)
            {
                AddLog(ex.Message);
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
                    catch(HttpRequestException)
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
            }
            catch(NavigationException){ }

            switch(true)
            {
                case true when lastResponse.Status == HttpStatusCode.TooManyRequests:
                    AddLog($"Ratelimit hit. Waiting around 3-5 seconds ... ({counter})");
                    AddLog("Try increasing the delay if you get this often.");
                    await Task.Delay(random.Next(3000, 5000));
                    break;

                case true when lastResponse.Status == HttpStatusCode.Forbidden:
                    AddLog($"Failed to retrieve the chapter. Status code : {lastResponse.StatusText}.");
                    AddLog($"Your IP might be banned. Retrying ... ({counter})");
                    await Task.Delay(random.Next(1000, 2000));
                    break;

                case true when lastResponse.Status == HttpStatusCode.NotFound:
                    throw new HttpRequestException("Error: 404 page not found. Aborting.", null, HttpStatusCode.NotFound);

                case true when !lastResponse.Ok:
                    AddLog("Last chapter request failed.");
                    AddLog("Status code : " + lastResponse.StatusText);
                    AddLog($"Retrying ... ({counter})");
                    await Task.Delay(random.Next(1000, 2000));
                    break;

                case true when page.Url.Contains("/view_uploads/"):
                    string html = await page.GetContentAsync();
                    string chapterId = Regex.Match(html, @"(?<=uniqid:.*'\b)(.*)(?=')").Value;

                    if(chapterId.Length == 0)
                    {
                        AddLog($"Failed to retrieve the chapter ID. It might be broken. Status code : {lastResponse.Status}.");
                        AddLog($"Retrying... ({counter}).");
                        await Task.Delay(random.Next(500, 1500));
                    } 
                    else
                    {
                        chapterLink = $"{DomainName}/viewer/{chapterId}/cascade";
                    }
                    break;

                case true when page.Url.Contains("/paginated"):
                    chapterLink = Regex.Replace(page.Url, "/paginated.*", "/cascade");
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
            bool goodToGo = false;

            while (!goodToGo)
            {
                cancellationToken.Token.ThrowIfCancellationRequested();

                doc = webClient.Load(url);

                switch (webClient.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new HttpRequestException("Error: 404 not found. Check your URL.", null, HttpStatusCode.NotFound);
                    case HttpStatusCode.TooManyRequests:
                        AddLog($"Ratelimit hit. Waiting around 3-5 seconds ... ({counter++})");
                        await Task.Delay(random.Next(3000, 5000));
                        break;
                    case HttpStatusCode.OK:
                        goodToGo = true;
                        break;
                    default:
                        AddLog("Loading page failed. Status code : " + webClient.StatusCode);
                        AddLog("Retrying ...");
                        await Task.Delay(random.Next(1000, 1500));
                        break;
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

        private void ListScanGroups(HtmlDocument doc)
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

            for (int x = 0; x < uploadNodes.Count; ++x)
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
            await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
            {
                { "Referer", DomainName }
            });
            await page.SetJavaScriptEnabledAsync(false);
            await page.SetRequestInterceptionAsync(true);
            await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:108.0) Gecko/20100101 Firefox/108.0");

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
                    case ResourceType.Script:
                        e.Request.AbortAsync();
                        break;
                    default:
                        e.Request.ContinueAsync();
                        break;
                }
            };
        }

        private void ToggleUI()
        {
            btn_setFolder.Enabled = !btn_setFolder.Enabled;
            listBox_Scannies.Enabled = !listBox_Scannies.Enabled;
            btn_scan.Enabled = !btn_scan.Enabled;
            btn_download.Enabled = !btn_download.Enabled;
            btn_stop.Enabled = !btn_stop.Enabled;
            btn_selectAllScannies.Enabled = !btn_selectAllScannies.Enabled;
        }

        private void AddLog(string message)
        {
            //listbox_logger.Items.Insert(0, message);
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            cancellationToken?.Cancel();
            AddLog("Post-natal abortion requested.");
        }

        private string CleanMangoTitle(string filename)
        {
            string title = string.Join(" ", WebUtility.HtmlDecode(filename).Split(Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).ToArray())).Trim().Replace(' ', '-');

            while (title.Last() == '.')
            {
                title = title.Remove(title.Length - 1, 1);
            }

            return title;
        }

        private void TxtBoxDelay_TextChanged(object sender, EventArgs e)
        {
            waitingTimeBetweenChapters = int.TryParse(txtBox_Delay.Text, out waitingTimeBetweenChapters) ? waitingTimeBetweenChapters : 3000;
        }

        private void TxtBoxMangoUrl_TextChanged(object sender, EventArgs e)
        {
            listBox_Scannies.Visible = false;
        }

        private void Btn_selectAllScannies_Click(object sender, EventArgs e)
        {
            if (listBox_Scannies.Visible)
            {
                for (int i = 0; i < listBox_Scannies.Items.Count; i++)
                {
                    listBox_Scannies.SetItemChecked(i, selectGroupsToggle);
                }

                btn_selectAllScannies.Text = selectGroupsToggle ? "Unselect all" : "Select all";
                selectGroupsToggle = !selectGroupsToggle;
            }
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

    public class LoggerTheme : RichTextBoxConsoleTheme
    {
        public LoggerTheme(IReadOnlyDictionary<RichTextBoxThemeStyle, RichTextBoxConsoleThemeStyle> styles) : base(styles)
        {
        }

        public static RichTextBoxConsoleTheme MyTheme { get; } = new RichTextBoxConsoleTheme
            (
                new Dictionary<RichTextBoxThemeStyle, RichTextBoxConsoleThemeStyle>
                {
                    [RichTextBoxThemeStyle.Text] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.Black.ToString() },
                    [RichTextBoxThemeStyle.SecondaryText] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.Black.ToString() },
                    [RichTextBoxThemeStyle.TertiaryText] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.Black.ToString() },
                    [RichTextBoxThemeStyle.Invalid] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.Orange.ToString() },
                    [RichTextBoxThemeStyle.Null] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.Blue.ToString() },
                    [RichTextBoxThemeStyle.Name] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.Black.ToString() },
                    [RichTextBoxThemeStyle.String] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.DarkCyan.ToString() },
                    [RichTextBoxThemeStyle.Number] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.DarkMagenta.ToString() },
                    [RichTextBoxThemeStyle.Boolean] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.DarkBlue.ToString() },
                    [RichTextBoxThemeStyle.Scalar] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.DarkGreen.ToString() },
                    [RichTextBoxThemeStyle.LevelVerbose] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.Gray.ToString() },
                    [RichTextBoxThemeStyle.LevelDebug] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.Blue.ToString() },
                    [RichTextBoxThemeStyle.LevelInformation] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.Blue.ToString() },
                    [RichTextBoxThemeStyle.LevelWarning] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.Orange.ToString() },
                    [RichTextBoxThemeStyle.LevelError] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.Red.ToString() },
                    [RichTextBoxThemeStyle.LevelFatal] = new RichTextBoxConsoleThemeStyle { Foreground = Brushes.Red.ToString() },
                }
            );
    }

    
}