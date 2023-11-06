namespace SpanishScraper
{
    partial class Form1
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
            this.btn_download = new System.Windows.Forms.Button();
            this.lbl_mangoUrl = new System.Windows.Forms.Label();
            this.txtBox_mangoUrl = new System.Windows.Forms.TextBox();
            this.txtBox_setFolder = new System.Windows.Forms.TextBox();
            this.btn_setFolder = new System.Windows.Forms.Button();
            this.setFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.btn_scan = new System.Windows.Forms.Button();
            this.listBox_Scannies = new System.Windows.Forms.CheckedListBox();
            this.checkBox_MangoSubfolder = new System.Windows.Forms.CheckBox();
            this.listbox_logger = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_stop = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBox_Delay = new System.Windows.Forms.TextBox();
            this.lbl_language = new System.Windows.Forms.Label();
            this.languageCmbBox = new System.Windows.Forms.ComboBox();
            this.checkBox_chaptersRange = new System.Windows.Forms.CheckBox();
            this.numeric_chaptersRangeFrom = new System.Windows.Forms.NumericUpDown();
            this.lbl_chaptersRangeTo = new System.Windows.Forms.Label();
            this.numeric_chaptersRangeTo = new System.Windows.Forms.NumericUpDown();
            this.checkBox_skipMangos = new System.Windows.Forms.CheckBox();
            this.numeric_skipMangos = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_chaptersRangeFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_chaptersRangeTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_skipMangos)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_download
            // 
            this.btn_download.Enabled = false;
            this.btn_download.Location = new System.Drawing.Point(21, 249);
            this.btn_download.Name = "btn_download";
            this.btn_download.Size = new System.Drawing.Size(119, 23);
            this.btn_download.TabIndex = 0;
            this.btn_download.Text = "Download";
            this.btn_download.UseVisualStyleBackColor = true;
            this.btn_download.Click += new System.EventHandler(this.Btn_download_Click);
            // 
            // lbl_mangoUrl
            // 
            this.lbl_mangoUrl.AutoSize = true;
            this.lbl_mangoUrl.Location = new System.Drawing.Point(23, 39);
            this.lbl_mangoUrl.Name = "lbl_mangoUrl";
            this.lbl_mangoUrl.Size = new System.Drawing.Size(75, 15);
            this.lbl_mangoUrl.TabIndex = 1;
            this.lbl_mangoUrl.Text = "Mango URL :";
            // 
            // txtBox_mangoUrl
            // 
            this.txtBox_mangoUrl.Location = new System.Drawing.Point(104, 36);
            this.txtBox_mangoUrl.Name = "txtBox_mangoUrl";
            this.txtBox_mangoUrl.Size = new System.Drawing.Size(441, 23);
            this.txtBox_mangoUrl.TabIndex = 2;
            this.txtBox_mangoUrl.TextChanged += new System.EventHandler(this.txtBox_mangoUrl_TextChanged);
            // 
            // txtBox_setFolder
            // 
            this.txtBox_setFolder.Enabled = false;
            this.txtBox_setFolder.Location = new System.Drawing.Point(104, 65);
            this.txtBox_setFolder.Name = "txtBox_setFolder";
            this.txtBox_setFolder.Size = new System.Drawing.Size(441, 23);
            this.txtBox_setFolder.TabIndex = 4;
            // 
            // btn_setFolder
            // 
            this.btn_setFolder.Location = new System.Drawing.Point(22, 65);
            this.btn_setFolder.Name = "btn_setFolder";
            this.btn_setFolder.Size = new System.Drawing.Size(76, 23);
            this.btn_setFolder.TabIndex = 5;
            this.btn_setFolder.Text = "Set Folder";
            this.btn_setFolder.UseVisualStyleBackColor = true;
            this.btn_setFolder.Click += new System.EventHandler(this.Btn_setFolder_Click);
            // 
            // btn_scan
            // 
            this.btn_scan.Enabled = false;
            this.btn_scan.Location = new System.Drawing.Point(21, 153);
            this.btn_scan.Name = "btn_scan";
            this.btn_scan.Size = new System.Drawing.Size(121, 23);
            this.btn_scan.TabIndex = 6;
            this.btn_scan.Text = "Scan the scannies";
            this.btn_scan.UseVisualStyleBackColor = true;
            this.btn_scan.Click += new System.EventHandler(this.Btn_scan_Click);
            // 
            // listBox_Scannies
            // 
            this.listBox_Scannies.FormattingEnabled = true;
            this.listBox_Scannies.Location = new System.Drawing.Point(148, 153);
            this.listBox_Scannies.Name = "listBox_Scannies";
            this.listBox_Scannies.Size = new System.Drawing.Size(397, 76);
            this.listBox_Scannies.TabIndex = 7;
            this.listBox_Scannies.Visible = false;
            // 
            // checkBox_MangoSubfolder
            // 
            this.checkBox_MangoSubfolder.AutoSize = true;
            this.checkBox_MangoSubfolder.Checked = true;
            this.checkBox_MangoSubfolder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_MangoSubfolder.Location = new System.Drawing.Point(391, 98);
            this.checkBox_MangoSubfolder.Name = "checkBox_MangoSubfolder";
            this.checkBox_MangoSubfolder.Size = new System.Drawing.Size(154, 19);
            this.checkBox_MangoSubfolder.TabIndex = 9;
            this.checkBox_MangoSubfolder.Text = "Create mango subfolder";
            this.checkBox_MangoSubfolder.UseVisualStyleBackColor = true;
            // 
            // listbox_logger
            // 
            this.listbox_logger.FormattingEnabled = true;
            this.listbox_logger.HorizontalScrollbar = true;
            this.listbox_logger.ItemHeight = 15;
            this.listbox_logger.Location = new System.Drawing.Point(148, 249);
            this.listbox_logger.Name = "listbox_logger";
            this.listbox_logger.Size = new System.Drawing.Size(397, 139);
            this.listbox_logger.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(251, 404);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 11;
            this.label1.Text = "Weird UI";
            // 
            // btn_stop
            // 
            this.btn_stop.Enabled = false;
            this.btn_stop.Location = new System.Drawing.Point(22, 289);
            this.btn_stop.Name = "btn_stop";
            this.btn_stop.Size = new System.Drawing.Size(118, 23);
            this.btn_stop.TabIndex = 12;
            this.btn_stop.Text = "Stop";
            this.btn_stop.UseVisualStyleBackColor = true;
            this.btn_stop.Click += new System.EventHandler(this.btn_stop_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 333);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 15);
            this.label2.TabIndex = 13;
            this.label2.Text = "Delay :";
            // 
            // txtBox_Delay
            // 
            this.txtBox_Delay.Location = new System.Drawing.Point(71, 330);
            this.txtBox_Delay.Name = "txtBox_Delay";
            this.txtBox_Delay.Size = new System.Drawing.Size(62, 23);
            this.txtBox_Delay.TabIndex = 14;
            this.txtBox_Delay.Text = "3000";
            this.txtBox_Delay.TextChanged += new System.EventHandler(this.txtBox_Delay_TextChanged);
            // 
            // lbl_language
            // 
            this.lbl_language.AutoSize = true;
            this.lbl_language.Location = new System.Drawing.Point(21, 97);
            this.lbl_language.Name = "lbl_language";
            this.lbl_language.Size = new System.Drawing.Size(65, 15);
            this.lbl_language.TabIndex = 15;
            this.lbl_language.Text = "Language :";
            // 
            // languageCmbBox
            // 
            this.languageCmbBox.FormattingEnabled = true;
            this.languageCmbBox.Location = new System.Drawing.Point(104, 94);
            this.languageCmbBox.Name = "languageCmbBox";
            this.languageCmbBox.Size = new System.Drawing.Size(121, 23);
            this.languageCmbBox.TabIndex = 16;
            // 
            // checkBox_chaptersRange
            // 
            this.checkBox_chaptersRange.AutoSize = true;
            this.checkBox_chaptersRange.Location = new System.Drawing.Point(23, 123);
            this.checkBox_chaptersRange.Name = "checkBox_chaptersRange";
            this.checkBox_chaptersRange.Size = new System.Drawing.Size(115, 19);
            this.checkBox_chaptersRange.TabIndex = 17;
            this.checkBox_chaptersRange.Text = "Chapters Range :";
            this.checkBox_chaptersRange.UseVisualStyleBackColor = true;
            // 
            // numeric_chaptersRangeFrom
            // 
            this.numeric_chaptersRangeFrom.DecimalPlaces = 2;
            this.numeric_chaptersRangeFrom.Location = new System.Drawing.Point(144, 122);
            this.numeric_chaptersRangeFrom.Maximum = new decimal(new int[] {
            998,
            0,
            0,
            0});
            this.numeric_chaptersRangeFrom.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numeric_chaptersRangeFrom.Name = "numeric_chaptersRangeFrom";
            this.numeric_chaptersRangeFrom.Size = new System.Drawing.Size(52, 23);
            this.numeric_chaptersRangeFrom.TabIndex = 18;
            this.numeric_chaptersRangeFrom.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lbl_chaptersRangeTo
            // 
            this.lbl_chaptersRangeTo.AutoSize = true;
            this.lbl_chaptersRangeTo.Location = new System.Drawing.Point(202, 124);
            this.lbl_chaptersRangeTo.Name = "lbl_chaptersRangeTo";
            this.lbl_chaptersRangeTo.Size = new System.Drawing.Size(18, 15);
            this.lbl_chaptersRangeTo.TabIndex = 19;
            this.lbl_chaptersRangeTo.Text = "to";
            // 
            // numeric_chaptersRangeTo
            // 
            this.numeric_chaptersRangeTo.DecimalPlaces = 2;
            this.numeric_chaptersRangeTo.Location = new System.Drawing.Point(226, 122);
            this.numeric_chaptersRangeTo.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numeric_chaptersRangeTo.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numeric_chaptersRangeTo.Name = "numeric_chaptersRangeTo";
            this.numeric_chaptersRangeTo.Size = new System.Drawing.Size(52, 23);
            this.numeric_chaptersRangeTo.TabIndex = 20;
            this.numeric_chaptersRangeTo.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // checkBox_skipMangos
            // 
            this.checkBox_skipMangos.AutoSize = true;
            this.checkBox_skipMangos.Location = new System.Drawing.Point(391, 123);
            this.checkBox_skipMangos.Name = "checkBox_skipMangos";
            this.checkBox_skipMangos.Size = new System.Drawing.Size(54, 19);
            this.checkBox_skipMangos.TabIndex = 21;
            this.checkBox_skipMangos.Text = "Skip :";
            this.checkBox_skipMangos.UseVisualStyleBackColor = true;
            // 
            // numeric_skipMangos
            // 
            this.numeric_skipMangos.Location = new System.Drawing.Point(451, 122);
            this.numeric_skipMangos.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numeric_skipMangos.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numeric_skipMangos.Name = "numeric_skipMangos";
            this.numeric_skipMangos.Size = new System.Drawing.Size(46, 23);
            this.numeric_skipMangos.TabIndex = 22;
            this.numeric_skipMangos.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(557, 430);
            this.Controls.Add(this.numeric_skipMangos);
            this.Controls.Add(this.checkBox_skipMangos);
            this.Controls.Add(this.numeric_chaptersRangeTo);
            this.Controls.Add(this.lbl_chaptersRangeTo);
            this.Controls.Add(this.numeric_chaptersRangeFrom);
            this.Controls.Add(this.checkBox_chaptersRange);
            this.Controls.Add(this.languageCmbBox);
            this.Controls.Add(this.lbl_language);
            this.Controls.Add(this.txtBox_Delay);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btn_stop);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listbox_logger);
            this.Controls.Add(this.checkBox_MangoSubfolder);
            this.Controls.Add(this.listBox_Scannies);
            this.Controls.Add(this.btn_scan);
            this.Controls.Add(this.btn_setFolder);
            this.Controls.Add(this.txtBox_setFolder);
            this.Controls.Add(this.txtBox_mangoUrl);
            this.Controls.Add(this.lbl_mangoUrl);
            this.Controls.Add(this.btn_download);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Spanish Scrapper";
            ((System.ComponentModel.ISupportInitialize)(this.numeric_chaptersRangeFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_chaptersRangeTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numeric_skipMangos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private ListBox listbox_logger;
        private Label label1;
        private Button btn_stop;
        private Label label2;
        private TextBox txtBox_Delay;
        private Label lbl_language;
        private ComboBox languageCmbBox;
        private CheckBox checkBox_chaptersRange;
        private NumericUpDown numeric_chaptersRangeFrom;
        private Label lbl_chaptersRangeTo;
        private NumericUpDown numeric_chaptersRangeTo;
        private CheckBox checkBox_skipMangos;
        private NumericUpDown numeric_skipMangos;
    }
}