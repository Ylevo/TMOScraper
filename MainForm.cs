using System.Windows.Forms.Integration;
using TMOScrapper.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System.Windows;
using Brushes = System.Windows.Media.Brushes;
using Serilog;
using Serilog.Sinks.RichTextBox.Themes;
using TMOScrapper.Core.PageFetcher;
using TMOScrapper.Properties;

namespace TMOScrapper
{
    public partial class MainForm : Form
    {
        private readonly IServiceProvider serviceProvider;
        private bool selectGroupsToggle = true;
        private CancellationTokenSource? cancellationToken;
        private System.Windows.Controls.RichTextBox? loggerBox;
        private static readonly object loggerSync = new object();

        public MainForm(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;

            InitializeComponent();
            SetupLogger();

            cmbBox_language.DataSource = new BindingSource(
                new Dictionary<string, string> { { "Spanish", "es" }, { "Spanish (LATAM)", "es-la" } },
                null);
            cmbBox_language.DisplayMember = "Key";
            cmbBox_language.ValueMember = "Value";
            txtBox_setFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void SetupLogger()
        {
            var richTextBoxHost = new ElementHost
            {
                Dock = DockStyle.Fill,
            };
            panel_logger.Controls.Add(richTextBoxHost);

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
            loggerBox = wpfRichTextBox;
            richTextBoxHost.Child = wpfRichTextBox;
            const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

            var logConf = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Logger(lc => lc
                    .Filter.ByExcluding(log => log.Level == Serilog.Events.LogEventLevel.Fatal)
                    .WriteTo.RichTextBox(wpfRichTextBox, theme: LoggerTheme.MyTheme, outputTemplate: outputTemplate, syncRoot: loggerSync));

            if (Settings.Default.FileLogging)
            {
                logConf.WriteTo.File(
                "Logs/log.txt",
                outputTemplate: outputTemplate,
                fileSizeLimitBytes: 50000000,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileTimeLimit: TimeSpan.FromDays(7)
                );
            }

            Log.Logger = logConf.CreateLogger();
        }

        private ScrapperHandler GetNewScrapper()
        {
            var scrapper = serviceProvider.GetRequiredService<ScrapperHandler>();
            cancellationToken = scrapper.GetTokenSource();

            return scrapper;
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
            chkBox_mangoSubfolder.Checked = Settings.Default.SubFolder;
            txtBox_setFolder.Text = Settings.Default.MainFolder;
            cmbBox_language.SelectedValue = Settings.Default.Language;
            await PuppeteerPageFetcher.InitializePuppeteer();
            btn_download.Enabled = true;
            btn_scan.Enabled = true;
        }

        private void BtnSetFolder_Click(object sender, EventArgs e)
        {
            if (setFolderDialog.ShowDialog() == DialogResult.OK)
            {
                txtBox_setFolder.Text = setFolderDialog.SelectedPath;
                Settings.Default.MainFolder = setFolderDialog.SelectedPath;
            }
        }

        private async void Btn_scan_Click(object sender, EventArgs e)
        {
            if (txtBox_mangoUrl.Text == String.Empty || txtBox_setFolder.Text == string.Empty)
            {
                Log.Error("URL and/or folder path is empty.");
                return;
            }

            loggerBox.Document.Blocks.Clear();
            ToggleUI();

            List<string>? groups = await GetNewScrapper().ScrapScanGroups(txtBox_mangoUrl.Text);

            if (groups != null)
            {
                listBox_scannies.Items.Clear();
                listBox_scannies.Items.AddRange(groups.ToArray());
                listBox_scannies.Visible = true;
            }

            ToggleUI();
        }

        private async void BtnDownload_Click(object sender, EventArgs e)
        {
            var groups = listBox_scannies.CheckedItems.Cast<string>().ToArray();

            loggerBox.Document.Blocks.Clear();
            ToggleUI();

            await GetNewScrapper().ScrapChapters(
                txtBox_mangoUrl.Text,
                groups.Length == 0 ? null : groups,
                (chkBox_chaptersRange.Checked, numeric_chaptersRangeFrom.Value, numeric_chaptersRangeTo.Value),
                chkBox_skipMangos.Checked ? (int)numeric_skipMangos.Value : 0
                );

            ToggleUI();
        }

        private void ToggleUI()
        {
            btn_setFolder.Enabled = !btn_setFolder.Enabled;
            listBox_scannies.Enabled = !listBox_scannies.Enabled;
            btn_scan.Enabled = !btn_scan.Enabled;
            btn_download.Enabled = !btn_download.Enabled;
            btn_stop.Enabled = !btn_stop.Enabled;
            btn_selectAllScannies.Enabled = !btn_selectAllScannies.Enabled;
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            if (cancellationToken != null && cancellationToken.Token.CanBeCanceled)
            {
                cancellationToken?.Cancel();
                Log.Warning("Abortion requested. Aborting ...");
            }
        }

        private void TxtBoxMangoUrl_TextChanged(object sender, EventArgs e)
        {
            listBox_scannies.Visible = false;
        }

        private void Btn_selectAllScannies_Click(object sender, EventArgs e)
        {
            if (listBox_scannies.Visible)
            {
                for (int i = 0; i < listBox_scannies.Items.Count; i++)
                {
                    listBox_scannies.SetItemChecked(i, selectGroupsToggle);
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

        private void MenuItemOptions_Click(object sender, EventArgs e)
        {
            new OptionsForm().ShowDialog(this);
        }

        private void chkBox_mangoSubfolder_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.SubFolder = chkBox_mangoSubfolder.Checked;
        }

        private void cmbBox_language_SelectionChangeCommitted(object sender, EventArgs e)
        {
            Settings.Default.Language = (string)cmbBox_language.SelectedValue;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.Save();
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