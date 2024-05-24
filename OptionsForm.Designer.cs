namespace TMOScrapper
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
            lbl_domain = new Label();
            txtBox_domain = new TextBox();
            lbl_puppeteer = new Label();
            cmbBox_puppeteer = new ComboBox();
            lbl_maxRetries = new Label();
            lbl_retryDelay = new Label();
            lbl_chapterDelay = new Label();
            chkBox_convert = new CheckBox();
            cmbBox_convert = new ComboBox();
            chkBox_crop = new CheckBox();
            numeric_chapterDelay = new NumericUpDown();
            numeric_retryDelay = new NumericUpDown();
            numeric_maxRetries = new NumericUpDown();
            btn_save = new Button();
            btn_cancel = new Button();
            chkBox_logger = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)numeric_chapterDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numeric_retryDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numeric_maxRetries).BeginInit();
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
            // 
            // lbl_maxRetries
            // 
            lbl_maxRetries.AutoSize = true;
            lbl_maxRetries.Location = new Point(259, 79);
            lbl_maxRetries.Name = "lbl_maxRetries";
            lbl_maxRetries.Size = new Size(74, 15);
            lbl_maxRetries.TabIndex = 4;
            lbl_maxRetries.Text = "Max Retries :";
            // 
            // lbl_retryDelay
            // 
            lbl_retryDelay.AutoSize = true;
            lbl_retryDelay.Location = new Point(259, 52);
            lbl_retryDelay.Name = "lbl_retryDelay";
            lbl_retryDelay.Size = new Size(72, 15);
            lbl_retryDelay.TabIndex = 5;
            lbl_retryDelay.Text = "Retry Delay :";
            // 
            // lbl_chapterDelay
            // 
            lbl_chapterDelay.AutoSize = true;
            lbl_chapterDelay.Location = new Point(259, 23);
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
            chkBox_convert.Text = "Convert Images";
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
            // 
            // chkBox_crop
            // 
            chkBox_crop.AutoSize = true;
            chkBox_crop.Location = new Point(12, 106);
            chkBox_crop.Name = "chkBox_crop";
            chkBox_crop.Size = new Size(93, 19);
            chkBox_crop.TabIndex = 9;
            chkBox_crop.Text = "Crop Images";
            chkBox_crop.UseVisualStyleBackColor = true;
            // 
            // numeric_chapterDelay
            // 
            numeric_chapterDelay.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            numeric_chapterDelay.Location = new Point(352, 20);
            numeric_chapterDelay.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numeric_chapterDelay.Name = "numeric_chapterDelay";
            numeric_chapterDelay.Size = new Size(80, 23);
            numeric_chapterDelay.TabIndex = 10;
            // 
            // numeric_retryDelay
            // 
            numeric_retryDelay.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            numeric_retryDelay.Location = new Point(352, 49);
            numeric_retryDelay.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numeric_retryDelay.Name = "numeric_retryDelay";
            numeric_retryDelay.Size = new Size(80, 23);
            numeric_retryDelay.TabIndex = 11;
            // 
            // numeric_maxRetries
            // 
            numeric_maxRetries.Location = new Point(352, 77);
            numeric_maxRetries.Name = "numeric_maxRetries";
            numeric_maxRetries.Size = new Size(80, 23);
            numeric_maxRetries.TabIndex = 12;
            // 
            // btn_save
            // 
            btn_save.Location = new Point(271, 148);
            btn_save.Name = "btn_save";
            btn_save.Size = new Size(75, 23);
            btn_save.TabIndex = 13;
            btn_save.Text = "Save";
            btn_save.UseVisualStyleBackColor = true;
            btn_save.Click += BtnSave_Click;
            // 
            // btn_cancel
            // 
            btn_cancel.Location = new Point(352, 148);
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
            chkBox_logger.UseVisualStyleBackColor = true;
            // 
            // OptionsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(443, 183);
            Controls.Add(chkBox_logger);
            Controls.Add(btn_cancel);
            Controls.Add(btn_save);
            Controls.Add(numeric_maxRetries);
            Controls.Add(numeric_retryDelay);
            Controls.Add(numeric_chapterDelay);
            Controls.Add(chkBox_crop);
            Controls.Add(cmbBox_convert);
            Controls.Add(chkBox_convert);
            Controls.Add(lbl_chapterDelay);
            Controls.Add(lbl_retryDelay);
            Controls.Add(lbl_maxRetries);
            Controls.Add(cmbBox_puppeteer);
            Controls.Add(lbl_puppeteer);
            Controls.Add(txtBox_domain);
            Controls.Add(lbl_domain);
            Name = "OptionsForm";
            Text = "OptionsForm";
            ((System.ComponentModel.ISupportInitialize)numeric_chapterDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numeric_retryDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numeric_maxRetries).EndInit();
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
        private CheckBox chkBox_crop;
        private NumericUpDown numeric_chapterDelay;
        private NumericUpDown numeric_retryDelay;
        private NumericUpDown numeric_maxRetries;
        private Button btn_save;
        private Button btn_cancel;
        private CheckBox chkBox_logger;
    }
}