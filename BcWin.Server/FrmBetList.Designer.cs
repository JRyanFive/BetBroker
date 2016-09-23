namespace BcWin.Server
{
    partial class FrmBetList
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
            this.web2 = new System.Windows.Forms.WebBrowser();
            this.web1 = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // web2
            // 
            this.web2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.web2.Location = new System.Drawing.Point(0, 407);
            this.web2.MinimumSize = new System.Drawing.Size(20, 20);
            this.web2.Name = "web2";
            this.web2.Size = new System.Drawing.Size(813, 390);
            this.web2.TabIndex = 0;
            // 
            // web1
            // 
            this.web1.Dock = System.Windows.Forms.DockStyle.Top;
            this.web1.Location = new System.Drawing.Point(0, 0);
            this.web1.MinimumSize = new System.Drawing.Size(20, 20);
            this.web1.Name = "web1";
            this.web1.Size = new System.Drawing.Size(813, 401);
            this.web1.TabIndex = 1;
            // 
            // FrmBetList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(813, 797);
            this.Controls.Add(this.web1);
            this.Controls.Add(this.web2);
            this.Name = "FrmBetList";
            this.Text = "FrmBetList";
            this.Load += new System.EventHandler(this.FrmBetList_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser web2;
        private System.Windows.Forms.WebBrowser web1;
    }
}