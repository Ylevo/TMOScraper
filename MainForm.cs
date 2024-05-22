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
using Newtonsoft.Json.Linq;
using TMOScrapper.Core.PageFetcher;

namespace TMOScrapper
{
    public partial class MainForm : Form
    {
        private readonly IServiceProvider serviceProvider;
        private bool selectGroupsToggle = true;
        private CancellationTokenSource? cancellationToken;
        private int waitingTimeBetweenChapters = 3000;

        private static readonly object loggerSync = new object();

        public MainForm(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;

            InitializeComponent();
            AddRichTextBoxLogger();

            languageCmbBox.DataSource = new BindingSource(
                new Dictionary<string, string> { { "Spanish", "es" }, { "Spanish (LATAM)", "es-la" } },
                null);
            languageCmbBox.DisplayMember = "Key";
            languageCmbBox.ValueMember = "Value";
            txtBox_setFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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
            wpfRichTextBox.TextChanged += delegate
            {
                wpfRichTextBox.ScrollToEnd();
            };

            richTextBoxHost.Child = wpfRichTextBox;
            const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RichTextBox(wpfRichTextBox, theme: LoggerTheme.MyTheme, outputTemplate: outputTemplate, syncRoot: loggerSync)
                .WriteTo.File(
                "Logs/log.txt", 
                outputTemplate: outputTemplate, 
                fileSizeLimitBytes: 50000000, 
                rollingInterval: RollingInterval.Day, 
                rollOnFileSizeLimit: true,
                retainedFileTimeLimit: TimeSpan.FromDays(7))
                .CreateLogger();
        }

        private ScrapperHandler GetNewScrapper()
        {
            var scrapper = serviceProvider.GetRequiredService<ScrapperHandler>();
            cancellationToken = scrapper.GetTokenSource();

            return scrapper;
        }
        
        private async void MainForm_Shown(object sender, EventArgs e)
        {
            await PuppeteerPageFetcher.InitializePuppeteer();
            btn_download.Enabled = true;
            btn_scan.Enabled = true;
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
                Log.Error("URL and/or folder path is empty.");
                return;
            }

            ToggleUI();
            List<string>? groups = await GetNewScrapper().ScrapScanGroups(txtBox_mangoUrl.Text);

            if (groups != null)
            {
                listBox_Scannies.Items.Clear();
                listBox_Scannies.Items.AddRange(groups.ToArray());
                listBox_Scannies.Visible = true;
            }

            ToggleUI();
        }

        private async void BtnDownload_Click(object sender, EventArgs e)
        {
            var groups = listBox_Scannies.CheckedItems.Cast<string>().ToArray();
            // clear logger?
            ToggleUI();

            await GetNewScrapper().StartScrapping(
                txtBox_mangoUrl.Text,
                groups.Length == 0 ? null : groups,
                (checkBox_chaptersRange.Checked, numeric_chaptersRangeFrom.Value, numeric_chaptersRangeTo.Value),
                (int)numeric_skipMangos.Value
                );

            ToggleUI();
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

        private void BtnStop_Click(object sender, EventArgs e)
        {
            cancellationToken?.Cancel();
            Log.Warning("Abortion requested. Aborting ...");
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
                PuppeteerPageFetcher.CloseBrowser();
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