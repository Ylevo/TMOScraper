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
            lblDomain = new Label();
            txtBoxDomain = new TextBox();
            lblPuppeteer = new Label();
            cmbBoxPuppeteer = new ComboBox();
            lblMaxRetries = new Label();
            lblRetryDelay = new Label();
            lblChapterDelay = new Label();
            chkBoxConvert = new CheckBox();
            cmbBoxConvert = new ComboBox();
            chkBoxCrop = new CheckBox();
            numericChapterDelay = new NumericUpDown();
            numericRetryDelay = new NumericUpDown();
            numericMaxRetries = new NumericUpDown();
            btnSave = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numericChapterDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericRetryDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericMaxRetries).BeginInit();
            SuspendLayout();
            // 
            // lblDomain
            // 
            lblDomain.AutoSize = true;
            lblDomain.Location = new Point(12, 23);
            lblDomain.Name = "lblDomain";
            lblDomain.Size = new Size(84, 15);
            lblDomain.TabIndex = 0;
            lblDomain.Text = "TMO Domain :";
            // 
            // txtBoxDomain
            // 
            txtBoxDomain.Location = new Point(107, 20);
            txtBoxDomain.Name = "txtBoxDomain";
            txtBoxDomain.Size = new Size(121, 23);
            txtBoxDomain.TabIndex = 1;
            // 
            // lblPuppeteer
            // 
            lblPuppeteer.AutoSize = true;
            lblPuppeteer.Location = new Point(12, 52);
            lblPuppeteer.Name = "lblPuppeteer";
            lblPuppeteer.Size = new Size(89, 15);
            lblPuppeteer.TabIndex = 2;
            lblPuppeteer.Text = "Use Puppeteer :";
            // 
            // cmbBoxPuppeteer
            // 
            cmbBoxPuppeteer.FormattingEnabled = true;
            cmbBoxPuppeteer.Items.AddRange(new object[] { "As fallback", "By default" });
            cmbBoxPuppeteer.Location = new Point(107, 49);
            cmbBoxPuppeteer.Name = "cmbBoxPuppeteer";
            cmbBoxPuppeteer.Size = new Size(121, 23);
            cmbBoxPuppeteer.TabIndex = 3;
            // 
            // lblMaxRetries
            // 
            lblMaxRetries.AutoSize = true;
            lblMaxRetries.Location = new Point(259, 79);
            lblMaxRetries.Name = "lblMaxRetries";
            lblMaxRetries.Size = new Size(74, 15);
            lblMaxRetries.TabIndex = 4;
            lblMaxRetries.Text = "Max Retries :";
            // 
            // lblRetryDelay
            // 
            lblRetryDelay.AutoSize = true;
            lblRetryDelay.Location = new Point(259, 52);
            lblRetryDelay.Name = "lblRetryDelay";
            lblRetryDelay.Size = new Size(72, 15);
            lblRetryDelay.TabIndex = 5;
            lblRetryDelay.Text = "Retry Delay :";
            // 
            // lblChapterDelay
            // 
            lblChapterDelay.AutoSize = true;
            lblChapterDelay.Location = new Point(259, 23);
            lblChapterDelay.Name = "lblChapterDelay";
            lblChapterDelay.Size = new Size(87, 15);
            lblChapterDelay.TabIndex = 6;
            lblChapterDelay.Text = "Chapter Delay :";
            // 
            // chkBoxConvert
            // 
            chkBoxConvert.AutoSize = true;
            chkBoxConvert.Location = new Point(12, 78);
            chkBoxConvert.Name = "chkBoxConvert";
            chkBoxConvert.Size = new Size(109, 19);
            chkBoxConvert.TabIndex = 7;
            chkBoxConvert.Text = "Convert Images";
            chkBoxConvert.UseVisualStyleBackColor = true;
            // 
            // cmbBoxConvert
            // 
            cmbBoxConvert.FormattingEnabled = true;
            cmbBoxConvert.Items.AddRange(new object[] { "JPEG", "PNG", "PNG 8 bits" });
            cmbBoxConvert.Location = new Point(127, 78);
            cmbBoxConvert.Name = "cmbBoxConvert";
            cmbBoxConvert.Size = new Size(86, 23);
            cmbBoxConvert.TabIndex = 8;
            // 
            // chkBoxCrop
            // 
            chkBoxCrop.AutoSize = true;
            chkBoxCrop.Location = new Point(12, 103);
            chkBoxCrop.Name = "chkBoxCrop";
            chkBoxCrop.Size = new Size(93, 19);
            chkBoxCrop.TabIndex = 9;
            chkBoxCrop.Text = "Crop Images";
            chkBoxCrop.UseVisualStyleBackColor = true;
            // 
            // numericChapterDelay
            // 
            numericChapterDelay.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            numericChapterDelay.Location = new Point(352, 20);
            numericChapterDelay.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numericChapterDelay.Name = "numericChapterDelay";
            numericChapterDelay.Size = new Size(80, 23);
            numericChapterDelay.TabIndex = 10;
            // 
            // numericRetryDelay
            // 
            numericRetryDelay.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            numericRetryDelay.Location = new Point(352, 49);
            numericRetryDelay.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numericRetryDelay.Name = "numericRetryDelay";
            numericRetryDelay.Size = new Size(80, 23);
            numericRetryDelay.TabIndex = 11;
            // 
            // numericMaxRetries
            // 
            numericMaxRetries.Location = new Point(352, 77);
            numericMaxRetries.Name = "numericMaxRetries";
            numericMaxRetries.Size = new Size(80, 23);
            numericMaxRetries.TabIndex = 12;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(271, 133);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 13;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += BtnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(352, 133);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 14;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // OptionsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(443, 168);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(numericMaxRetries);
            Controls.Add(numericRetryDelay);
            Controls.Add(numericChapterDelay);
            Controls.Add(chkBoxCrop);
            Controls.Add(cmbBoxConvert);
            Controls.Add(chkBoxConvert);
            Controls.Add(lblChapterDelay);
            Controls.Add(lblRetryDelay);
            Controls.Add(lblMaxRetries);
            Controls.Add(cmbBoxPuppeteer);
            Controls.Add(lblPuppeteer);
            Controls.Add(txtBoxDomain);
            Controls.Add(lblDomain);
            Name = "OptionsForm";
            Text = "OptionsForm";
            ((System.ComponentModel.ISupportInitialize)numericChapterDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericRetryDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericMaxRetries).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblDomain;
        private TextBox txtBoxDomain;
        private Label lblPuppeteer;
        private ComboBox cmbBoxPuppeteer;
        private Label lblMaxRetries;
        private Label lblRetryDelay;
        private Label lblChapterDelay;
        private CheckBox chkBoxConvert;
        private ComboBox cmbBoxConvert;
        private CheckBox chkBoxCrop;
        private NumericUpDown numericChapterDelay;
        private NumericUpDown numericRetryDelay;
        private NumericUpDown numericMaxRetries;
        private Button btnSave;
        private Button btnCancel;
    }
}