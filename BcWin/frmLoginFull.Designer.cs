﻿namespace BcWin
{
    partial class frmLoginFull
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLoginFull));
            this.lblInfo = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtIpAddress = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numTab = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numTab)).BeginInit();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo.ForeColor = System.Drawing.Color.Red;
            this.lblInfo.Location = new System.Drawing.Point(32, 11);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(41, 13);
            this.lblInfo.TabIndex = 11;
            this.lblInfo.Text = "label3";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(61, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Mật khẩu:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Tên đăng nhập:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(220, 117);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 29);
            this.button1.TabIndex = 8;
            this.button1.Text = "Đăng Nhập";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(122, 84);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(201, 20);
            this.txtPassword.TabIndex = 7;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(122, 44);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(201, 20);
            this.txtUsername.TabIndex = 6;
            // 
            // txtIpAddress
            // 
            this.txtIpAddress.Location = new System.Drawing.Point(35, 157);
            this.txtIpAddress.Name = "txtIpAddress";
            this.txtIpAddress.Size = new System.Drawing.Size(288, 117);
            this.txtIpAddress.TabIndex = 12;
            this.txtIpAddress.Text = "";
            this.txtIpAddress.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(61, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "TAB:";
            this.label3.Visible = false;
            // 
            // numTab
            // 
            this.numTab.Location = new System.Drawing.Point(122, 123);
            this.numTab.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numTab.Name = "numTab";
            this.numTab.Size = new System.Drawing.Size(120, 20);
            this.numTab.TabIndex = 13;
            this.numTab.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numTab.Visible = false;
            // 
            // frmLoginFull
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::BcWin.Properties.Resources._100_billion_of_climate_finance;
            this.ClientSize = new System.Drawing.Size(355, 166);
            this.Controls.Add(this.numTab);
            this.Controls.Add(this.txtIpAddress);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtUsername);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmLoginFull";
            this.Text = "Login";
            ((System.ComponentModel.ISupportInitialize)(this.numTab)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.RichTextBox txtIpAddress;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numTab;
    }
}