using HtmlAgilityPack;
using PuppeteerSharp;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.XPath;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace SpanishScraper
{
    public partial class Form1 : Form
    {
        private HttpClient httpClient;
        private HtmlWeb webClient;
        private IBrowser? browser;
        private readonly NavigationOptions navigationOptionsDefault = new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.DOMContentLoaded } };
        private readonly NavigationOptions navigationOptionsRedirect = new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle2 } };
        private HtmlDocument doc = new HtmlDocument();
        private Random random = new();
        private bool canceled = false;
        private int waitingTimeBetweenChapters = 2000;
        private const string domainName = "https://visortmo.com";
        private const string folderNameTemplate = "{0} [{1}] - c{2} [{3}]";
        private readonly Dictionary<string, string> langDict = new()
        {
            {"Spanish ","es" },
            {"Spanish (LATAM) ","es-la" }
        };
        public Form1()
        {
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

            var socketsHttpHandler = new SocketsHttpHandler()
            {
                MaxConnectionsPerServer = 5
            };
            httpClient = new HttpClient(socketsHttpHandler);
            webClient = new HtmlWeb();
            webClient.PreRequest += delegate (HttpWebRequest req)
            {
                req.Referer = domainName;
                return true;
            };
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:108.0) Gecko/20100101 Firefox/108.0");
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.Add("Referer", domainName);
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

        private void Btn_scan_Click(object sender, EventArgs e)
        {
            if (txtBox_mangoUrl.Text == String.Empty || txtBox_setFolder.Text == string.Empty)
            {
                MessageBox.Show("Retard.", "Retard Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Hand);
                return;
            }
            doc = webClient.Load(txtBox_mangoUrl.Text);
            ListScantardsGroups(doc);
        }
        
        private async void Btn_download_Click(object sender, EventArgs e)
        {
            if (!listBox_Scannies.Visible)
            {
                MessageBox.Show("You need to scan the scannies first to select your favourite autists.", "Scannies Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Hand);
                return;
            }
            ToggleButtonsAndShit();
            doc = webClient.Load(txtBox_mangoUrl.Text);
            Dictionary<string, (string GroupName, string ChapterLink)[]> chapters = GetChaptersLinks(doc);
            string mainFolder = txtBox_setFolder.Text,
                mangaTitle = ReplaceInvalidChars(txtBox_mangoUrl.Text.Split('/').Last()).Truncate(20),
                language = languageCmbBox.SelectedValue.ToString(),
                chapterNumber,
                currentFolder;
            bool actuallyDidSomething = false;
            if (checkBox_MangoSubfolder.Checked)
            {
                mainFolder = Path.Combine(mainFolder, mangaTitle);
                Directory.CreateDirectory(mainFolder);
            }
            try
            {
                foreach (var chapter in chapters.Reverse())
                {
                    AddLog("Checking chapter " + chapter.Key);
                    foreach (var uploadedChapter in chapter.Value)
                    {
                        if (canceled)
                        {
                            throw new TaskCanceledException();
                        }

                        actuallyDidSomething = false;

                        if (listBox_Scannies.CheckedItems.Contains(uploadedChapter.GroupName))
                        {
                            chapterNumber = chapter.Key.Contains('.') ? chapter.Key.PadLeft(6, '0') : chapter.Key.PadLeft(3, '0');
                            currentFolder = Path.Combine(mainFolder, String.Format(folderNameTemplate, mangaTitle, language, chapterNumber, uploadedChapter.GroupName));

                            if (Directory.Exists(currentFolder))
                            {
                                AddLog("Skipping chapter " + chapter.Key + " by '" + uploadedChapter.GroupName + "'. Folder already exists.");
                                continue;
                            }
                            Directory.CreateDirectory(currentFolder);
                            AddLog("Downloading chapter " + chapter.Key + " by '" + uploadedChapter.GroupName + "'");
                            await DownloadChapter(uploadedChapter.ChapterLink, currentFolder);
                            AddLog("Done downloading chapter " + chapter.Key + " by '" + uploadedChapter.GroupName + "'");
                            actuallyDidSomething = true;
                        }
                    }

                    if (actuallyDidSomething)
                    {
                        AddLog("Waiting " + waitingTimeBetweenChapters + " ms ...");
                        await Task.Delay(waitingTimeBetweenChapters);
                    }
                }

            }
            catch(TaskCanceledException)
            {
                AddLog("Stopped download.");
            }
            catch(Exception exc)
            {
                AddLog(exc.Message);
                AddLog("Something went wrong. Probably going too fast.");
            }
            finally
            {
                ToggleButtonsAndShit();
            }
        }

        private async Task DownloadChapter(string chapterLink, string folderPath)
        {
            await GetChapterPage(chapterLink);

            var imgUrls = doc.DocumentNode.SelectNodes("//img[contains(concat(' ',normalize-space(@class),' '),' viewer-img ')]");
            string url, filename;
            var tasks = new List<Task>();
            for (int i=0 ; i < imgUrls.Count; i++)
            {
                url = imgUrls[i].Attributes["data-src"].Value;
                filename = $"{i:D3}." + url.Split('.').Last();
                tasks.Add(DownloadFile(new Uri(url), Path.Combine(folderPath, filename), filename));
            }
            await Task.WhenAll(tasks);
        }

        private async Task GetChapterPage(string chapterLink)
        {
            using (var page = await browser.NewPageAsync())
            {
                await page.SetExtraHttpHeadersAsync(new Dictionary<string, string> { { "Referer", domainName } });
                await page.SetJavaScriptEnabledAsync(true);
                await page.SetRequestInterceptionAsync(true);

                page.Request += (sender, e) =>
                {
                    switch(e.Request.ResourceType)
                    {
                        case ResourceType.Image:
                        case ResourceType.Img:
                        case ResourceType.StyleSheet:
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

                await NavigateToChapterPage(chapterLink, page);
                doc.LoadHtml(await page.GetContentAsync());
            }
        }

        private async Task NavigateToChapterPage(string chapterLink, IPage page)
        {
            await page.GoToAsync(chapterLink, navigationOptionsDefault);
            if (page.Url.Contains("/view_uploads/"))
            {
                await page.WaitForNavigationAsync(navigationOptionsRedirect);
            }

            if (!page.Url.Contains(domainName + "/viewer/") || !page.Url.Contains("/cascade"))
            {
                chapterLink = page.Url.Replace("paginated", "cascade");
                chapterLink = Regex.Replace(chapterLink, "https.+/news/", domainName + "/viewer/");
                await NavigateToChapterPage(chapterLink, page);
            }
        }

        private async Task DownloadFile(Uri uri, string path, string filename)
        {
            using (var s = await httpClient.GetStreamAsync(uri))
            {
                if (canceled)
                {
                    throw new TaskCanceledException();
                }

                using (var fs = new FileStream(path, FileMode.CreateNew))
                {
                    AddLog("Downloading file " + filename + " ...");
                    await s.CopyToAsync(fs);
                }
            }
        }

        private void ListScantardsGroups(HtmlDocument doc)
        {
            List<string> scanGroups = new();
            var scanGroupsNodes = doc.DocumentNode.SelectNodes(@"//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]
                                                                    //a[contains(@href, '/groups/')]");
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

        private Dictionary<string, (string, string)[]> GetChaptersLinks(HtmlDocument doc)
        {
            Dictionary<string, (string, string)[]> chapters = new();
            var chaptersNodes = doc.DocumentNode.SelectNodes("//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]");
            IEnumerable<HtmlNode> uploadedChaptersNodes;
            string chapterNumber, uploadedChapterLink, groupName;
            for (int i = 0; i < chaptersNodes.Count; ++i)
            {
                chapterNumber = chaptersNodes[i].Descendants("a").First().InnerText.Trim().Substring(9);

                if (int.Parse(chapterNumber.Substring(chapterNumber.IndexOf(".") + 1, 2)) > 0)
                {
                    chapterNumber = chapterNumber.Substring(0, chapterNumber.IndexOf(".") + 3);
                }
                else
                {
                    chapterNumber = chapterNumber.Substring(0, chapterNumber.IndexOf("."));
                }

                uploadedChaptersNodes = chaptersNodes[i].Descendants("li");
                (string, string)[] uploadedChapters = new (string, string)[uploadedChaptersNodes.Count()];
                
                for (int x = 0; x < uploadedChaptersNodes.Count(); ++x)
                {
                    uploadedChapterLink = uploadedChaptersNodes.ElementAt(x).Descendants("a").Last().Attributes["href"].Value;
                    groupName = String.Join('+', uploadedChaptersNodes.ElementAt(x).Descendants("a").First().ParentNode.InnerText.Split(',', StringSplitOptions.TrimEntries));
                    uploadedChapters[x] = (groupName, uploadedChapterLink);
                }
                chapters.Add(chapterNumber, uploadedChapters);
            }

            return chapters;
        }

        private void ToggleButtonsAndShit()
        {
            btn_setFolder.Enabled = !btn_setFolder.Enabled;
            listBox_Scannies.Enabled = !listBox_Scannies.Enabled;
            btn_scan.Enabled = !btn_scan.Enabled;
            btn_download.Enabled = !btn_download.Enabled;
            btn_stop.Enabled = !btn_stop.Enabled;
            canceled = false;

        }

        private void AddLog(string message)
        {
            listbox_logger.Items.Insert(0, message);
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            canceled = true;
        }

        private string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        private void txtBox_Delay_TextChanged(object sender, EventArgs e)
        {
            waitingTimeBetweenChapters = int.TryParse(txtBox_Delay.Text, out waitingTimeBetweenChapters) ? waitingTimeBetweenChapters : 2000;
        }

        private void txtBox_mangoUrl_TextChanged(object sender, EventArgs e)
        {
            listBox_Scannies.Visible = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await GetChapterPage("https://visortmo.com/view_uploads/165301");
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