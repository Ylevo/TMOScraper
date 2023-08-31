using HtmlAgilityPack;
using System.Net;
using System.Windows.Forms;
using System.Xml.XPath;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace SpanishScraper
{
    public partial class Form1 : Form
    {
        private HttpClient httpClient;
        private HtmlWeb webClient;
        private HtmlDocument doc;
        private bool canceled = false;
        private int waitingTime = 1500;
        public Form1()
        {
            var socketsHttpHandler = new SocketsHttpHandler()
            {
                MaxConnectionsPerServer = 5
            };
            httpClient = new HttpClient(socketsHttpHandler);
            webClient = new HtmlWeb();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:108.0) Gecko/20100101 Firefox/108.0");
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.Add("Referer", "https://lectortmo.com/");
            InitializeComponent();
            txtBox_setFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

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
            toggleButtonsAndShit();
            doc = webClient.Load(txtBox_mangoUrl.Text);
            Dictionary<string, (string GroupName, string ChapterLink)[]> chapters = GetChaptersLinks(doc);
            string mainFolder = txtBox_setFolder.Text;
            string currentFolder;
            if (checkBox_MangoSubfolder.Checked)
            {
                mainFolder = Path.Combine(mainFolder, ReplaceInvalidChars(txtBox_mangoUrl.Text.Split('/').Last()));
                Directory.CreateDirectory(mainFolder);
            }
            try
            {
                foreach (var chapter in chapters.Reverse())
                {
                    currentFolder = Path.Combine(mainFolder, ReplaceInvalidChars(chapter.Key));
                    if (Directory.Exists(currentFolder)) { addLog("Skipping chapter - folder already exists."); continue; }
                    Directory.CreateDirectory(currentFolder);
                    addLog("Downloading chapter " + chapter.Key);
                    foreach (var uploadedChapter in chapter.Value)
                    {
                        if (canceled)
                        {
                            throw new TaskCanceledException();
                        }
                        if (listBox_Scannies.CheckedItems.Contains(uploadedChapter.GroupName))
                        {
                            addLog("Downloading chapter by '" + uploadedChapter.GroupName + "'");
                            await DownloadChapter(uploadedChapter.ChapterLink, Path.Combine(currentFolder, ReplaceInvalidChars(uploadedChapter.GroupName)));
                            addLog("Done.");
                        }
                    }
                    addLog("Done downloading chapter " + chapter.Key);
                    addLog("Waiting " + waitingTime + " ms ...");
                    await Task.Delay(waitingTime);
                }

            }
            catch(TaskCanceledException)
            {
                addLog("Stopped download.");
            }
            catch(Exception exc)
            {
                addLog(exc.Message);
                addLog("Something went wrong. Probably going too fast.");
            }
            finally
            {
                toggleButtonsAndShit();
            }
        }

        private async Task DownloadChapter(string chapterLink, string folderPath)
        {
            Directory.CreateDirectory(folderPath);
            doc = webClient.Load(chapterLink);
            if (webClient.ResponseUri.ToString().Split('/').Last() != "cascade")
            {
                string cascadeUrl = doc.DocumentNode.SelectSingleNode("//a[contains(@href, 'cascade')]").Attributes["href"].Value;
                doc = webClient.Load(cascadeUrl);
            }
            var imgUrls = doc.DocumentNode.SelectNodes("//img[contains(concat(' ',normalize-space(@class),' '),' viewer-img ')]");
            string url, filename;
            var tasks = new List<Task>();
            foreach (var imgUrl in imgUrls)
            {
                url = imgUrl.Attributes["data-src"].Value;
                filename = url.Split('/').Last(); 
                tasks.Add(DownloadFile(new Uri(url), Path.Combine(folderPath, filename), filename)); 
                
            }
            await Task.WhenAll(tasks);
        }

        private async Task DownloadFile(Uri uri, string path, string filename)
        {
            if (File.Exists(path)) 
            {
                addLog("Skipping file '" + filename + "' - already exists.");
                return;
            }

            var s = await httpClient.GetStreamAsync(uri);
            
            if (canceled)
            {
                throw new TaskCanceledException();
            }
            using (var fs = new FileStream(path, FileMode.CreateNew))
            {
                addLog("Downloading file " + filename + " ...");
                await s.CopyToAsync(fs);
            }

        }

        private void ListScantardsGroups(HtmlDocument doc)
        {
            List<string> scanGroups = new List<string>();
            var scanGroupsNodes = doc.DocumentNode.SelectNodes(@"//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]
                                                                    //a[starts-with(@href, 'https://lectortmo.com/groups/')]");
            foreach (var scanGroupNode in scanGroupsNodes)
            {
                scanGroups.Add(scanGroupNode.InnerText);
            }
            scanGroups = scanGroups.Distinct().ToList();
            scanGroups.Sort();
            listBox_Scannies.Items.Clear();
            listBox_Scannies.Items.AddRange(scanGroups.ToArray());
            listBox_Scannies.Visible = true;
        }

        private Dictionary<string, (string, string)[]> GetChaptersLinks(HtmlDocument doc)
        {
            Dictionary<string, (string, string)[]> chapters = new Dictionary<string, (string, string)[]>();
            var chaptersNodes = doc.DocumentNode.SelectNodes("//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]");
            IEnumerable<HtmlNode> uploadedChaptersNodes;
            IEnumerable<HtmlNode> uploadedChapterLinksNodes;
            string chapterName;
            for (int i = 0; i < chaptersNodes.Count; ++i)
            {
                chapterName = chaptersNodes[i].Descendants("a").First().InnerText.Trim();
                uploadedChaptersNodes = chaptersNodes[i].Descendants("li");
                (string, string)[] uploadedChapters = new (string, string)[uploadedChaptersNodes.Count()];
                
                for (int x = 0; x < uploadedChaptersNodes.Count(); ++x)
                {
                    uploadedChapterLinksNodes = uploadedChaptersNodes.ElementAt(x).Descendants("a");
                    uploadedChapters[x] = (uploadedChapterLinksNodes.First().InnerText, uploadedChapterLinksNodes.Last().Attributes["href"].Value);
                }
                chapters.Add(chapterName, uploadedChapters);
            }

            return chapters;
        }

        private void toggleButtonsAndShit()
        {
            btn_setFolder.Enabled = !btn_setFolder.Enabled;
            listBox_Scannies.Enabled = !listBox_Scannies.Enabled;
            btn_scan.Enabled = !btn_scan.Enabled;
            btn_download.Enabled = !btn_download.Enabled;
            btn_stop.Enabled = !btn_stop.Enabled;
            canceled = false;

        }

        private void addLog(string message)
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
            waitingTime = int.TryParse(txtBox_Delay.Text, out waitingTime) ? waitingTime : 1500;
        }

        private void txtBox_mangoUrl_TextChanged(object sender, EventArgs e)
        {
            listBox_Scannies.Visible = false;
        }
    }
}