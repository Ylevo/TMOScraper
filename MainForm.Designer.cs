namespace TMOScrapper
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>


        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btn_download = new Button();
            lbl_mangoUrl = new Label();
            txtBox_mangoUrl = new TextBox();
            txtBox_setFolder = new TextBox();
            btn_setFolder = new Button();
            setFolderDialog = new FolderBrowserDialog();
            btn_scan = new Button();
            listBox_Scannies = new CheckedListBox();
            checkBox_MangoSubfolder = new CheckBox();
            label1 = new Label();
            btn_stop = new Button();
            lbl_language = new Label();
            languageCmbBox = new ComboBox();
            checkBox_chaptersRange = new CheckBox();
            numeric_chaptersRangeFrom = new NumericUpDown();
            lbl_chaptersRangeTo = new Label();
            numeric_chaptersRangeTo = new NumericUpDown();
            checkBox_skipMangos = new CheckBox();
            numeric_skipMangos = new NumericUpDown();
            btn_selectAllScannies = new Button();
            panel_Logger = new Panel();
            menuStrip1 = new MenuStrip();
            menuItemOptions = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)numeric_chaptersRangeFrom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numeric_chaptersRangeTo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numeric_skipMangos).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btn_download
            // 
            btn_download.Enabled = false;
            btn_download.Location = new Point(12, 247);
            btn_download.Name = "btn_download";
            btn_download.Size = new Size(119, 23);
            btn_download.TabIndex = 0;
            btn_download.Text = "Download";
            btn_download.UseVisualStyleBackColor = true;
            btn_download.Click += BtnDownload_Click;
            // 
            // lbl_mangoUrl
            // 
            lbl_mangoUrl.AutoSize = true;
            lbl_mangoUrl.Location = new Point(14, 37);
            lbl_mangoUrl.Name = "lbl_mangoUrl";
            lbl_mangoUrl.Size = new Size(75, 15);
            lbl_mangoUrl.TabIndex = 1;
            lbl_mangoUrl.Text = "Mango URL :";
            // 
            // txtBox_mangoUrl
            // 
            txtBox_mangoUrl.Location = new Point(95, 34);
            txtBox_mangoUrl.Name = "txtBox_mangoUrl";
            txtBox_mangoUrl.Size = new Size(441, 23);
            txtBox_mangoUrl.TabIndex = 2;
            txtBox_mangoUrl.TextChanged += TxtBoxMangoUrl_TextChanged;
            // 
            // txtBox_setFolder
            // 
            txtBox_setFolder.Enabled = false;
            txtBox_setFolder.Location = new Point(95, 63);
            txtBox_setFolder.Name = "txtBox_setFolder";
            txtBox_setFolder.Size = new Size(441, 23);
            txtBox_setFolder.TabIndex = 4;
            // 
            // btn_setFolder
            // 
            btn_setFolder.Location = new Point(13, 63);
            btn_setFolder.Name = "btn_setFolder";
            btn_setFolder.Size = new Size(76, 23);
            btn_setFolder.TabIndex = 5;
            btn_setFolder.Text = "Set Folder";
            btn_setFolder.UseVisualStyleBackColor = true;
            btn_setFolder.Click += BtnSetFolder_Click;
            // 
            // btn_scan
            // 
            btn_scan.Enabled = false;
            btn_scan.Location = new Point(12, 164);
            btn_scan.Name = "btn_scan";
            btn_scan.Size = new Size(121, 23);
            btn_scan.TabIndex = 6;
            btn_scan.Text = "Scan the scannies";
            btn_scan.UseVisualStyleBackColor = true;
            btn_scan.Click += Btn_scan_Click;
            // 
            // listBox_Scannies
            // 
            listBox_Scannies.FormattingEnabled = true;
            listBox_Scannies.Location = new Point(139, 151);
            listBox_Scannies.Name = "listBox_Scannies";
            listBox_Scannies.Size = new Size(397, 76);
            listBox_Scannies.TabIndex = 7;
            listBox_Scannies.Visible = false;
            // 
            // checkBox_MangoSubfolder
            // 
            checkBox_MangoSubfolder.AutoSize = true;
            checkBox_MangoSubfolder.Checked = true;
            checkBox_MangoSubfolder.CheckState = CheckState.Checked;
            checkBox_MangoSubfolder.Location = new Point(382, 96);
            checkBox_MangoSubfolder.Name = "checkBox_MangoSubfolder";
            checkBox_MangoSubfolder.Size = new Size(154, 19);
            checkBox_MangoSubfolder.TabIndex = 9;
            checkBox_MangoSubfolder.Text = "Create mango subfolder";
            checkBox_MangoSubfolder.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(242, 402);
            label1.Name = "label1";
            label1.Size = new Size(52, 15);
            label1.TabIndex = 11;
            label1.Text = "Weird UI";
            // 
            // btn_stop
            // 
            btn_stop.Enabled = false;
            btn_stop.Location = new Point(13, 276);
            btn_stop.Name = "btn_stop";
            btn_stop.Size = new Size(118, 23);
            btn_stop.TabIndex = 12;
            btn_stop.Text = "Stop";
            btn_stop.UseVisualStyleBackColor = true;
            btn_stop.Click += BtnStop_Click;
            // 
            // lbl_language
            // 
            lbl_language.AutoSize = true;
            lbl_language.Location = new Point(12, 95);
            lbl_language.Name = "lbl_language";
            lbl_language.Size = new Size(65, 15);
            lbl_language.TabIndex = 15;
            lbl_language.Text = "Language :";
            // 
            // languageCmbBox
            // 
            languageCmbBox.FormattingEnabled = true;
            languageCmbBox.Location = new Point(95, 92);
            languageCmbBox.Name = "languageCmbBox";
            languageCmbBox.Size = new Size(121, 23);
            languageCmbBox.TabIndex = 16;
            // 
            // checkBox_chaptersRange
            // 
            checkBox_chaptersRange.AutoSize = true;
            checkBox_chaptersRange.Location = new Point(14, 121);
            checkBox_chaptersRange.Name = "checkBox_chaptersRange";
            checkBox_chaptersRange.Size = new Size(115, 19);
            checkBox_chaptersRange.TabIndex = 17;
            checkBox_chaptersRange.Text = "Chapters Range :";
            checkBox_chaptersRange.UseVisualStyleBackColor = true;
            // 
            // numeric_chaptersRangeFrom
            // 
            numeric_chaptersRangeFrom.DecimalPlaces = 2;
            numeric_chaptersRangeFrom.Location = new Point(135, 120);
            numeric_chaptersRangeFrom.Maximum = new decimal(new int[] { 998, 0, 0, 0 });
            numeric_chaptersRangeFrom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numeric_chaptersRangeFrom.Name = "numeric_chaptersRangeFrom";
            numeric_chaptersRangeFrom.Size = new Size(52, 23);
            numeric_chaptersRangeFrom.TabIndex = 18;
            numeric_chaptersRangeFrom.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lbl_chaptersRangeTo
            // 
            lbl_chaptersRangeTo.AutoSize = true;
            lbl_chaptersRangeTo.Location = new Point(193, 122);
            lbl_chaptersRangeTo.Name = "lbl_chaptersRangeTo";
            lbl_chaptersRangeTo.Size = new Size(18, 15);
            lbl_chaptersRangeTo.TabIndex = 19;
            lbl_chaptersRangeTo.Text = "to";
            // 
            // numeric_chaptersRangeTo
            // 
            numeric_chaptersRangeTo.DecimalPlaces = 2;
            numeric_chaptersRangeTo.Location = new Point(217, 120);
            numeric_chaptersRangeTo.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            numeric_chaptersRangeTo.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            numeric_chaptersRangeTo.Name = "numeric_chaptersRangeTo";
            numeric_chaptersRangeTo.Size = new Size(52, 23);
            numeric_chaptersRangeTo.TabIndex = 20;
            numeric_chaptersRangeTo.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // checkBox_skipMangos
            // 
            checkBox_skipMangos.AutoSize = true;
            checkBox_skipMangos.Location = new Point(382, 121);
            checkBox_skipMangos.Name = "checkBox_skipMangos";
            checkBox_skipMangos.Size = new Size(54, 19);
            checkBox_skipMangos.TabIndex = 21;
            checkBox_skipMangos.Text = "Skip :";
            checkBox_skipMangos.UseVisualStyleBackColor = true;
            // 
            // numeric_skipMangos
            // 
            numeric_skipMangos.Location = new Point(442, 120);
            numeric_skipMangos.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            numeric_skipMangos.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numeric_skipMangos.Name = "numeric_skipMangos";
            numeric_skipMangos.Size = new Size(46, 23);
            numeric_skipMangos.TabIndex = 22;
            numeric_skipMangos.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btn_selectAllScannies
            // 
            btn_selectAllScannies.Location = new Point(12, 193);
            btn_selectAllScannies.Name = "btn_selectAllScannies";
            btn_selectAllScannies.Size = new Size(121, 23);
            btn_selectAllScannies.TabIndex = 23;
            btn_selectAllScannies.Text = "Select all";
            btn_selectAllScannies.UseVisualStyleBackColor = true;
            btn_selectAllScannies.Click += Btn_selectAllScannies_Click;
            // 
            // panel_Logger
            // 
            panel_Logger.Location = new Point(139, 246);
            panel_Logger.Name = "panel_Logger";
            panel_Logger.Size = new Size(397, 138);
            panel_Logger.TabIndex = 24;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { menuItemOptions });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(557, 24);
            menuStrip1.TabIndex = 25;
            menuStrip1.Text = "menuStrip1";
            // 
            // menuItemOptions
            // 
            menuItemOptions.Name = "menuItemOptions";
            menuItemOptions.Size = new Size(61, 20);
            menuItemOptions.Text = "Options";
            menuItemOptions.Click += MenuItemOptions_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(557, 430);
            Controls.Add(panel_Logger);
            Controls.Add(btn_selectAllScannies);
            Controls.Add(numeric_skipMangos);
            Controls.Add(checkBox_skipMangos);
            Controls.Add(numeric_chaptersRangeTo);
            Controls.Add(lbl_chaptersRangeTo);
            Controls.Add(numeric_chaptersRangeFrom);
            Controls.Add(checkBox_chaptersRange);
            Controls.Add(languageCmbBox);
            Controls.Add(lbl_language);
            Controls.Add(btn_stop);
            Controls.Add(label1);
            Controls.Add(checkBox_MangoSubfolder);
            Controls.Add(listBox_Scannies);
            Controls.Add(btn_scan);
            Controls.Add(btn_setFolder);
            Controls.Add(txtBox_setFolder);
            Controls.Add(txtBox_mangoUrl);
            Controls.Add(lbl_mangoUrl);
            Controls.Add(btn_download);
            Controls.Add(menuStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            Name = "MainForm";
            Text = "TMO Scrapper";
            Shown += MainForm_Shown;
            ((System.ComponentModel.ISupportInitialize)numeric_chaptersRangeFrom).EndInit();
            ((System.ComponentModel.ISupportInitialize)numeric_chaptersRangeTo).EndInit();
            ((System.ComponentModel.ISupportInitialize)numeric_skipMangos).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_download;
        private Label lbl_mangoUrl;
        private TextBox txtBox_mangoUrl;
        private TextBox txtBox_setFolder;
        private Button btn_setFolder;
        private FolderBrowserDialog setFolderDialog;
        private Button btn_scan;
        private CheckedListBox listBox_Scannies;
        private CheckBox checkBox_MangoSubfolder;
        private Label label1;
        private Button btn_stop;
        private Label lbl_language;
        private ComboBox languageCmbBox;
        private CheckBox checkBox_chaptersRange;
        private NumericUpDown numeric_chaptersRangeFrom;
        private Label lbl_chaptersRangeTo;
        private NumericUpDown numeric_chaptersRangeTo;
        private CheckBox checkBox_skipMangos;
        private NumericUpDown numeric_skipMangos;
        private Button btn_selectAllScannies;
        private Panel panel_Logger;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuItemOptions;
    }
}