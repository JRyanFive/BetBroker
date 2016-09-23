namespace BcWin.Server
{
    partial class FrmService
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
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabPageStartPage = new System.Windows.Forms.TabPage();
            this.btnStopService = new System.Windows.Forms.Button();
            this.btnStartService = new System.Windows.Forms.Button();
            this.tabMain.SuspendLayout();
            this.tabPageStartPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabPageStartPage);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(729, 626);
            this.tabMain.TabIndex = 0;
            // 
            // tabPageStartPage
            // 
            this.tabPageStartPage.Controls.Add(this.btnStopService);
            this.tabPageStartPage.Controls.Add(this.btnStartService);
            this.tabPageStartPage.Location = new System.Drawing.Point(4, 22);
            this.tabPageStartPage.Name = "tabPageStartPage";
            this.tabPageStartPage.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageStartPage.Size = new System.Drawing.Size(721, 600);
            this.tabPageStartPage.TabIndex = 0;
            this.tabPageStartPage.Text = "Start Page";
            this.tabPageStartPage.UseVisualStyleBackColor = true;
            // 
            // btnStopService
            // 
            this.btnStopService.Location = new System.Drawing.Point(8, 35);
            this.btnStopService.Name = "btnStopService";
            this.btnStopService.Size = new System.Drawing.Size(75, 23);
            this.btnStopService.TabIndex = 1;
            this.btnStopService.Text = "Stop";
            this.btnStopService.UseVisualStyleBackColor = true;
            this.btnStopService.Click += new System.EventHandler(this.btnStopService_Click);
            // 
            // btnStartService
            // 
            this.btnStartService.Location = new System.Drawing.Point(8, 6);
            this.btnStartService.Name = "btnStartService";
            this.btnStartService.Size = new System.Drawing.Size(75, 23);
            this.btnStartService.TabIndex = 0;
            this.btnStartService.Text = "Start";
            this.btnStartService.UseVisualStyleBackColor = true;
            this.btnStartService.Click += new System.EventHandler(this.btnStartService_Click);
            // 
            // FrmService
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(729, 626);
            this.Controls.Add(this.tabMain);
            this.Name = "FrmService";
            this.Text = "FrmService";
            this.tabMain.ResumeLayout(false);
            this.tabPageStartPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabPageStartPage;
        private System.Windows.Forms.Button btnStopService;
        private System.Windows.Forms.Button btnStartService;
    }
}