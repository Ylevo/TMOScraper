using TMOScrapper.Properties;

namespace TMOScrapper
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();

            txtBox_domain.Text = Settings.Default.Domain;
            numeric_chapterDelay.Value = Settings.Default.ChapterDelay;
            numeric_mangoDelay.Value = Settings.Default.MangoDelay;
            numeric_retryDelay.Value = Settings.Default.RetryDelay;
            numeric_maxRetries.Value = Settings.Default.MaxRetries;
            cmbBox_puppeteer.SelectedIndex = Settings.Default.AlwaysUsePuppeteer ? cmbBox_puppeteer.FindStringExact("By default") : cmbBox_puppeteer.FindStringExact("As fallback");
            cmbBox_convert.SelectedIndex = cmbBox_convert.FindStringExact(Settings.Default.ConvertFormat);
            chkBox_convert.Checked = Settings.Default.ConvertImages;
            chkBox_split.Checked = Settings.Default.SplitImages;
            chkBox_logger.Checked = Settings.Default.FileLogging;
            chkBox_verboseLogging.Checked = Settings.Default.VerboseLogging;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Settings.Default.Domain = txtBox_domain.Text;
            Settings.Default.ChapterDelay = (int)numeric_chapterDelay.Value;
            Settings.Default.MangoDelay = (int)numeric_mangoDelay.Value;
            Settings.Default.RetryDelay = (int)numeric_retryDelay.Value;
            Settings.Default.MaxRetries = (int)numeric_maxRetries.Value;
            Settings.Default.AlwaysUsePuppeteer = (string)cmbBox_puppeteer.SelectedItem == "By default";
            Settings.Default.ConvertFormat = (string)cmbBox_convert.SelectedItem;
            Settings.Default.ConvertImages = chkBox_convert.Checked;
            Settings.Default.SplitImages = chkBox_split.Checked;
            Settings.Default.FileLogging = chkBox_logger.Checked;
            Settings.Default.VerboseLogging = chkBox_verboseLogging.Checked;

            Settings.Default.Save();
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
