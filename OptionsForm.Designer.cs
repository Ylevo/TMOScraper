namespace TMOScraper
{
    partial class OptionsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
            lbl_domain = new Label();
            txtBox_domain = new TextBox();
            lbl_puppeteer = new Label();
            cmbBox_puppeteer = new ComboBox();
            lbl_maxRetries = new Label();
            lbl_retryDelay = new Label();
            lbl_chapterDelay = new Label();
            chkBox_convert = new CheckBox();
            cmbBox_convert = new ComboBox();
            chkBox_split = new CheckBox();
            numeric_chapterDelay = new NumericUpDown();
            numeric_retryDelay = new NumericUpDown();
            numeric_maxRetries = new NumericUpDown();
            btn_save = new Button();
            btn_cancel = new Button();
            chkBox_logger = new CheckBox();
            lbl_mangoDelay = new Label();
            numeric_mangoDelay = new NumericUpDown();
            toolTip_optionsForm = new ToolTip(components);
            chkBox_verboseLogging = new CheckBox();
            chkBox_scrapChapterTitles = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)numeric_chapterDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numeric_retryDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numeric_maxRetries).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numeric_mangoDelay).BeginInit();
            SuspendLayout();
            // 
            // lbl_domain
            // 
            lbl_domain.AutoSize = true;
            lbl_domain.Location = new Point(12, 23);
            lbl_domain.Name = "lbl_domain";
            lbl_domain.Size = new Size(84, 15);
            lbl_domain.TabIndex = 0;
            lbl_domain.Text = "TMO Domain :";
            // 
            // txtBox_domain
            // 
            txtBox_domain.Location = new Point(107, 20);
            txtBox_domain.Name = "txtBox_domain";
            txtBox_domain.Size = new Size(121, 23);
            txtBox_domain.TabIndex = 1;
            toolTip_optionsForm.SetToolTip(txtBox_domain, "Don't change it unless TMO changed theirs.");
            // 
            // lbl_puppeteer
            // 
            lbl_puppeteer.AutoSize = true;
            lbl_puppeteer.Location = new Point(12, 52);
            lbl_puppeteer.Name = "lbl_puppeteer";
            lbl_puppeteer.Size = new Size(89, 15);
            lbl_puppeteer.TabIndex = 2;
            lbl_puppeteer.Text = "Use Puppeteer :";
            // 
            // cmbBox_puppeteer
            // 
            cmbBox_puppeteer.FormattingEnabled = true;
            cmbBox_puppeteer.Items.AddRange(new object[] { "As fallback", "By default" });
            cmbBox_puppeteer.Location = new Point(107, 49);
            cmbBox_puppeteer.Name = "cmbBox_puppeteer";
            cmbBox_puppeteer.Size = new Size(121, 23);
            cmbBox_puppeteer.TabIndex = 3;
            toolTip_optionsForm.SetToolTip(cmbBox_puppeteer, resources.GetString("cmbBox_puppeteer.ToolTip"));
            // 
            // lbl_maxRetries
            // 
            lbl_maxRetries.AutoSize = true;
            lbl_maxRetries.Location = new Point(259, 109);
            lbl_maxRetries.Name = "lbl_maxRetries";
            lbl_maxRetries.Size = new Size(74, 15);
            lbl_maxRetries.TabIndex = 4;
            lbl_maxRetries.Text = "Max Retries :";
            // 
            // lbl_retryDelay
            // 
            lbl_retryDelay.AutoSize = true;
            lbl_retryDelay.Location = new Point(259, 80);
            lbl_retryDelay.Name = "lbl_retryDelay";
            lbl_retryDelay.Size = new Size(72, 15);
            lbl_retryDelay.TabIndex = 5;
            lbl_retryDelay.Text = "Retry Delay :";
            // 
            // lbl_chapterDelay
            // 
            lbl_chapterDelay.AutoSize = true;
            lbl_chapterDelay.Location = new Point(259, 22);
            lbl_chapterDelay.Name = "lbl_chapterDelay";
            lbl_chapterDelay.Size = new Size(87, 15);
            lbl_chapterDelay.TabIndex = 6;
            lbl_chapterDelay.Text = "Chapter Delay :";
            // 
            // chkBox_convert
            // 
            chkBox_convert.AutoSize = true;
            chkBox_convert.Location = new Point(12, 81);
            chkBox_convert.Name = "chkBox_convert";
            chkBox_convert.Size = new Size(109, 19);
            chkBox_convert.TabIndex = 7;
            chkBox_convert.Text = "Convert images";
            toolTip_optionsForm.SetToolTip(chkBox_convert, "When checked, will convert images to the picked format.");
            chkBox_convert.UseVisualStyleBackColor = true;
            // 
            // cmbBox_convert
            // 
            cmbBox_convert.FormattingEnabled = true;
            cmbBox_convert.Items.AddRange(new object[] { "JPEG", "PNG", "PNG 4 bpp" });
            cmbBox_convert.Location = new Point(127, 78);
            cmbBox_convert.Name = "cmbBox_convert";
            cmbBox_convert.Size = new Size(101, 23);
            cmbBox_convert.TabIndex = 8;
            toolTip_optionsForm.SetToolTip(cmbBox_convert, "JPEG quality is set to 90.\r\nUse PNG 8 bpp for best compression if the chapter(s) only have B&W images.\r\nOtherwise, use JPEG. Or PNG if you like wasting space.");
            // 
            // chkBox_split
            // 
            chkBox_split.AutoSize = true;
            chkBox_split.Location = new Point(12, 106);
            chkBox_split.Name = "chkBox_split";
            chkBox_split.Size = new Size(90, 19);
            chkBox_split.TabIndex = 9;
            chkBox_split.Text = "Split images";
            toolTip_optionsForm.SetToolTip(chkBox_split, "When checked, will split images longer than 10000 pixels in height.");
            chkBox_split.UseVisualStyleBackColor = true;
            // 
            // numeric_chapterDelay
            // 
            numeric_chapterDelay.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            numeric_chapterDelay.Location = new Point(361, 20);
            numeric_chapterDelay.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numeric_chapterDelay.Name = "numeric_chapterDelay";
            numeric_chapterDelay.Size = new Size(80, 23);
            numeric_chapterDelay.TabIndex = 10;
            toolTip_optionsForm.SetToolTip(numeric_chapterDelay, "Time in milliseconds to wait before scraping the next chapter.");
            // 
            // numeric_retryDelay
            // 
            numeric_retryDelay.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            numeric_retryDelay.Location = new Point(361, 78);
            numeric_retryDelay.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numeric_retryDelay.Name = "numeric_retryDelay";
            numeric_retryDelay.Size = new Size(80, 23);
            numeric_retryDelay.TabIndex = 11;
            toolTip_optionsForm.SetToolTip(numeric_retryDelay, "Time in milliseconds to wait before retrying to fetch a page.");
            // 
            // numeric_maxRetries
            // 
            numeric_maxRetries.Location = new Point(361, 107);
            numeric_maxRetries.Name = "numeric_maxRetries";
            numeric_maxRetries.Size = new Size(80, 23);
            numeric_maxRetries.TabIndex = 12;
            toolTip_optionsForm.SetToolTip(numeric_maxRetries, "Maximum number of retries when trying to fetch a page.");
            // 
            // btn_save
            // 
            btn_save.Location = new Point(285, 197);
            btn_save.Name = "btn_save";
            btn_save.Size = new Size(75, 23);
            btn_save.TabIndex = 0;
            btn_save.Text = "Save";
            btn_save.UseVisualStyleBackColor = true;
            btn_save.Click += BtnSave_Click;
            // 
            // btn_cancel
            // 
            btn_cancel.Location = new Point(366, 197);
            btn_cancel.Name = "btn_cancel";
            btn_cancel.Size = new Size(75, 23);
            btn_cancel.TabIndex = 14;
            btn_cancel.Text = "Cancel";
            btn_cancel.UseVisualStyleBackColor = true;
            btn_cancel.Click += BtnCancel_Click;
            // 
            // chkBox_logger
            // 
            chkBox_logger.AutoSize = true;
            chkBox_logger.Location = new Point(12, 131);
            chkBox_logger.Name = "chkBox_logger";
            chkBox_logger.Size = new Size(124, 19);
            chkBox_logger.TabIndex = 15;
            chkBox_logger.Text = "Enable file logging";
            toolTip_optionsForm.SetToolTip(chkBox_logger, "When checked, will save the logs to a text file under the folder Logs.");
            chkBox_logger.UseVisualStyleBackColor = true;
            // 
            // lbl_mangoDelay
            // 
            lbl_mangoDelay.AutoSize = true;
            lbl_mangoDelay.Location = new Point(259, 51);
            lbl_mangoDelay.Name = "lbl_mangoDelay";
            lbl_mangoDelay.Size = new Size(83, 15);
            lbl_mangoDelay.TabIndex = 16;
            lbl_mangoDelay.Text = "Mango Delay :";
            // 
            // numeric_mangoDelay
            // 
            numeric_mangoDelay.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            numeric_mangoDelay.Location = new Point(361, 49);
            numeric_mangoDelay.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numeric_mangoDelay.Name = "numeric_mangoDelay";
            numeric_mangoDelay.Size = new Size(80, 23);
            numeric_mangoDelay.TabIndex = 17;
            toolTip_optionsForm.SetToolTip(numeric_mangoDelay, "Time in milliseconds to wait before scraping the next mango.");
            // 
            // toolTip_optionsForm
            // 
            toolTip_optionsForm.AutoPopDelay = 12000;
            toolTip_optionsForm.InitialDelay = 350;
            toolTip_optionsForm.ReshowDelay = 250;
            // 
            // chkBox_verboseLogging
            // 
            chkBox_verboseLogging.AutoSize = true;
            chkBox_verboseLogging.Location = new Point(12, 156);
            chkBox_verboseLogging.Name = "chkBox_verboseLogging";
            chkBox_verboseLogging.Size = new Size(149, 19);
            chkBox_verboseLogging.TabIndex = 18;
            chkBox_verboseLogging.Text = "Enable verbose logging";
            toolTip_optionsForm.SetToolTip(chkBox_verboseLogging, "When checked, will log files changes such as download/convert/split individually.");
            chkBox_verboseLogging.UseVisualStyleBackColor = true;
            // 
            // chkBox_scrapChapterTitles
            // 
            chkBox_scrapChapterTitles.AutoSize = true;
            chkBox_scrapChapterTitles.Location = new Point(12, 181);
            chkBox_scrapChapterTitles.Name = "chkBox_scrapChapterTitles";
            chkBox_scrapChapterTitles.Size = new Size(126, 19);
            chkBox_scrapChapterTitles.TabIndex = 19;
            chkBox_scrapChapterTitles.Text = "Scrap chapter titles";
            toolTip_optionsForm.SetToolTip(chkBox_scrapChapterTitles, "When checked, will log files changes such as download/convert/split individually.");
            chkBox_scrapChapterTitles.UseVisualStyleBackColor = true;
            // 
            // OptionsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(453, 232);
            Controls.Add(chkBox_scrapChapterTitles);
            Controls.Add(chkBox_verboseLogging);
            Controls.Add(numeric_mangoDelay);
            Controls.Add(lbl_mangoDelay);
            Controls.Add(chkBox_logger);
            Controls.Add(btn_cancel);
            Controls.Add(btn_save);
            Controls.Add(numeric_maxRetries);
            Controls.Add(numeric_retryDelay);
            Controls.Add(numeric_chapterDelay);
            Controls.Add(chkBox_split);
            Controls.Add(cmbBox_convert);
            Controls.Add(chkBox_convert);
            Controls.Add(lbl_chapterDelay);
            Controls.Add(lbl_retryDelay);
            Controls.Add(lbl_maxRetries);
            Controls.Add(cmbBox_puppeteer);
            Controls.Add(lbl_puppeteer);
            Controls.Add(txtBox_domain);
            Controls.Add(lbl_domain);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "OptionsForm";
            Text = "Options";
            ((System.ComponentModel.ISupportInitialize)numeric_chapterDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numeric_retryDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numeric_maxRetries).EndInit();
            ((System.ComponentModel.ISupportInitialize)numeric_mangoDelay).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_domain;
        private TextBox txtBox_domain;
        private Label lbl_puppeteer;
        private ComboBox cmbBox_puppeteer;
        private Label lbl_maxRetries;
        private Label lbl_retryDelay;
        private Label lbl_chapterDelay;
        private CheckBox chkBox_convert;
        private ComboBox cmbBox_convert;
        private CheckBox chkBox_split;
        private NumericUpDown numeric_chapterDelay;
        private NumericUpDown numeric_retryDelay;
        private NumericUpDown numeric_maxRetries;
        private Button btn_save;
        private Button btn_cancel;
        private CheckBox chkBox_logger;
        private Label lbl_mangoDelay;
        private NumericUpDown numeric_mangoDelay;
        private ToolTip toolTip_optionsForm;
        private CheckBox chkBox_verboseLogging;
        private CheckBox chkBox_scrapChapterTitles;
    }
}