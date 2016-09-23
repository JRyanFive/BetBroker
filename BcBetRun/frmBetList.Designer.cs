namespace BcBetRun
{
    partial class frmBetList
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
            this.webSboStatement = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webSboStatement
            // 
            this.webSboStatement.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webSboStatement.Location = new System.Drawing.Point(0, 0);
            this.webSboStatement.MinimumSize = new System.Drawing.Size(20, 20);
            this.webSboStatement.Name = "webSboStatement";
            this.webSboStatement.ScriptErrorsSuppressed = true;
            this.webSboStatement.Size = new System.Drawing.Size(949, 361);
            this.webSboStatement.TabIndex = 1;
            // 
            // frmBetList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(949, 361);
            this.Controls.Add(this.webSboStatement);
            this.Name = "frmBetList";
            this.Text = "frmBetList";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webSboStatement;
    }
}