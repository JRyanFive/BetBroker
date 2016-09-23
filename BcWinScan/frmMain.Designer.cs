namespace BcWinScan
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnIbetSboStart = new System.Windows.Forms.Button();
            this.lblStartTime = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.cboMarket = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnIbetSboStart
            // 
            this.btnIbetSboStart.Location = new System.Drawing.Point(73, 171);
            this.btnIbetSboStart.Name = "btnIbetSboStart";
            this.btnIbetSboStart.Size = new System.Drawing.Size(126, 43);
            this.btnIbetSboStart.TabIndex = 0;
            this.btnIbetSboStart.Text = "Ibet-Sbo Start";
            this.btnIbetSboStart.UseVisualStyleBackColor = true;
            this.btnIbetSboStart.Click += new System.EventHandler(this.btnIbetSboStart_Click);
            // 
            // lblStartTime
            // 
            this.lblStartTime.AutoSize = true;
            this.lblStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStartTime.ForeColor = System.Drawing.Color.Maroon;
            this.lblStartTime.Location = new System.Drawing.Point(149, 9);
            this.lblStartTime.Name = "lblStartTime";
            this.lblStartTime.Size = new System.Drawing.Size(41, 13);
            this.lblStartTime.TabIndex = 5;
            this.lblStartTime.Text = "label1";
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsername.ForeColor = System.Drawing.Color.Maroon;
            this.lblUsername.Location = new System.Drawing.Point(52, 9);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(41, 13);
            this.lblUsername.TabIndex = 6;
            this.lblUsername.Text = "label1";
            // 
            // cboMarket
            // 
            this.cboMarket.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMarket.FormattingEnabled = true;
            this.cboMarket.Items.AddRange(new object[] {
            "Live",
            "NonLive",
            "All"});
            this.cboMarket.Location = new System.Drawing.Point(83, 58);
            this.cboMarket.Name = "cboMarket";
            this.cboMarket.Size = new System.Drawing.Size(82, 21);
            this.cboMarket.TabIndex = 26;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(15, 61);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(43, 13);
            this.label13.TabIndex = 25;
            this.label13.Text = "Market:";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(716, 495);
            this.Controls.Add(this.cboMarket);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.lblStartTime);
            this.Controls.Add(this.btnIbetSboStart);
            this.Name = "frmMain";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnIbetSboStart;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.ComboBox cboMarket;
        private System.Windows.Forms.Label label13;
    }
}

