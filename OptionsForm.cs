using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TMOScrapper.Properties;

namespace TMOScrapper
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();

            txtBoxDomain.Text = Settings.Default.Domain;
            numericChapterDelay.Value = Settings.Default.ChapterDelay;
            numericRetryDelay.Value = Settings.Default.RetryDelay;
            numericMaxRetries.Value = Settings.Default.MaxRetries;
            cmbBoxPuppeteer.SelectedIndex = Settings.Default.AlwaysUsePuppeteer ? cmbBoxPuppeteer.FindStringExact("By default") : cmbBoxPuppeteer.FindStringExact("As fallback");
            cmbBoxConvert.SelectedIndex = cmbBoxConvert.FindStringExact(Settings.Default.ConvertFormat);
            chkBoxConvert.Checked = Settings.Default.ConvertImages;
            chkBoxCrop.Checked = Settings.Default.CropImages;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Settings.Default.Domain = txtBoxDomain.Text;
            Settings.Default.ChapterDelay = (int)numericChapterDelay.Value;
            Settings.Default.RetryDelay = (int)numericRetryDelay.Value;
            Settings.Default.MaxRetries = (int)numericMaxRetries.Value;
            Settings.Default.AlwaysUsePuppeteer = (string)cmbBoxPuppeteer.SelectedItem == "By default";
            Settings.Default.ConvertFormat = (string)cmbBoxConvert.SelectedItem;
            Settings.Default.ConvertImages = chkBoxConvert.Checked;
            Settings.Default.CropImages = chkBoxCrop.Checked;

            Settings.Default.Save();
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
