namespace BcManagement
{
    partial class frmMain
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
            this.btnIbetFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
            this.btnEncrypt = new System.Windows.Forms.Button();
            this.txtEncryp = new System.Windows.Forms.TextBox();
            this.btnLoadBetClientConfig = new System.Windows.Forms.Button();
            this.btnLoadScanConfig = new System.Windows.Forms.Button();
            this.btnAddNewAcc = new System.Windows.Forms.Button();
            this.btnSboFile = new System.Windows.Forms.Button();
            this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.xtraTabControl1.SuspendLayout();
            this.xtraTabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnIbetFile
            // 
            this.btnIbetFile.Location = new System.Drawing.Point(766, 492);
            this.btnIbetFile.Name = "btnIbetFile";
            this.btnIbetFile.Size = new System.Drawing.Size(111, 23);
            this.btnIbetFile.TabIndex = 0;
            this.btnIbetFile.Text = "Import Ibet File";
            this.btnIbetFile.UseVisualStyleBackColor = true;
            this.btnIbetFile.Click += new System.EventHandler(this.btnIbetFile_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xtraTabControl1.Location = new System.Drawing.Point(0, 0);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.SelectedTabPage = this.xtraTabPage1;
            this.xtraTabControl1.Size = new System.Drawing.Size(890, 550);
            this.xtraTabControl1.TabIndex = 1;
            this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPage1,
            this.xtraTabPage2});
            // 
            // xtraTabPage1
            // 
            this.xtraTabPage1.Controls.Add(this.btnEncrypt);
            this.xtraTabPage1.Controls.Add(this.txtEncryp);
            this.xtraTabPage1.Controls.Add(this.btnLoadBetClientConfig);
            this.xtraTabPage1.Controls.Add(this.btnLoadScanConfig);
            this.xtraTabPage1.Controls.Add(this.btnAddNewAcc);
            this.xtraTabPage1.Controls.Add(this.btnSboFile);
            this.xtraTabPage1.Controls.Add(this.btnIbetFile);
            this.xtraTabPage1.Name = "xtraTabPage1";
            this.xtraTabPage1.Size = new System.Drawing.Size(884, 522);
            this.xtraTabPage1.Text = "xtraTabPage1";
            // 
            // btnEncrypt
            // 
            this.btnEncrypt.Location = new System.Drawing.Point(715, 170);
            this.btnEncrypt.Name = "btnEncrypt";
            this.btnEncrypt.Size = new System.Drawing.Size(75, 23);
            this.btnEncrypt.TabIndex = 6;
            this.btnEncrypt.Text = "Encrypt";
            this.btnEncrypt.UseVisualStyleBackColor = true;
            this.btnEncrypt.Click += new System.EventHandler(this.btnEncrypt_Click);
            // 
            // txtEncryp
            // 
            this.txtEncryp.Location = new System.Drawing.Point(11, 170);
            this.txtEncryp.Name = "txtEncryp";
            this.txtEncryp.Size = new System.Drawing.Size(678, 20);
            this.txtEncryp.TabIndex = 5;
            // 
            // btnLoadBetClientConfig
            // 
            this.btnLoadBetClientConfig.Location = new System.Drawing.Point(513, 449);
            this.btnLoadBetClientConfig.Name = "btnLoadBetClientConfig";
            this.btnLoadBetClientConfig.Size = new System.Drawing.Size(126, 23);
            this.btnLoadBetClientConfig.TabIndex = 4;
            this.btnLoadBetClientConfig.Text = "Load Bet Client Config";
            this.btnLoadBetClientConfig.UseVisualStyleBackColor = true;
            this.btnLoadBetClientConfig.Click += new System.EventHandler(this.btnLoadBetClientConfig_Click);
            // 
            // btnLoadScanConfig
            // 
            this.btnLoadScanConfig.Location = new System.Drawing.Point(513, 492);
            this.btnLoadScanConfig.Name = "btnLoadScanConfig";
            this.btnLoadScanConfig.Size = new System.Drawing.Size(126, 23);
            this.btnLoadScanConfig.TabIndex = 3;
            this.btnLoadScanConfig.Text = "Load Scan Config";
            this.btnLoadScanConfig.UseVisualStyleBackColor = true;
            this.btnLoadScanConfig.Click += new System.EventHandler(this.btnLoadScanConfig_Click);
            // 
            // btnAddNewAcc
            // 
            this.btnAddNewAcc.Location = new System.Drawing.Point(751, 386);
            this.btnAddNewAcc.Name = "btnAddNewAcc";
            this.btnAddNewAcc.Size = new System.Drawing.Size(126, 23);
            this.btnAddNewAcc.TabIndex = 2;
            this.btnAddNewAcc.Text = "Add New User";
            this.btnAddNewAcc.UseVisualStyleBackColor = true;
            this.btnAddNewAcc.Click += new System.EventHandler(this.btnAddNewAcc_Click);
            // 
            // btnSboFile
            // 
            this.btnSboFile.Location = new System.Drawing.Point(766, 449);
            this.btnSboFile.Name = "btnSboFile";
            this.btnSboFile.Size = new System.Drawing.Size(111, 23);
            this.btnSboFile.TabIndex = 1;
            this.btnSboFile.Text = "Import Sbo File";
            this.btnSboFile.UseVisualStyleBackColor = true;
            this.btnSboFile.Click += new System.EventHandler(this.btnSboFile_Click);
            // 
            // xtraTabPage2
            // 
            this.xtraTabPage2.Name = "xtraTabPage2";
            this.xtraTabPage2.Size = new System.Drawing.Size(884, 522);
            this.xtraTabPage2.Text = "xtraTabPage2";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(890, 550);
            this.Controls.Add(this.xtraTabControl1);
            this.Name = "frmMain";
            this.Text = "bc management";
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            this.xtraTabControl1.ResumeLayout(false);
            this.xtraTabPage1.ResumeLayout(false);
            this.xtraTabPage1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnIbetFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage1;
        private System.Windows.Forms.Button btnSboFile;
        private System.Windows.Forms.Button btnAddNewAcc;
        private System.Windows.Forms.Button btnLoadScanConfig;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage2;
        private System.Windows.Forms.Button btnLoadBetClientConfig;
        private System.Windows.Forms.Button btnEncrypt;
        private System.Windows.Forms.TextBox txtEncryp;
    }
}

