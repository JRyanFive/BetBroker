namespace BcWinScan
{
    partial class frmLogin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtScanType = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtOddCompare = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numIbetLiveTime = new System.Windows.Forms.NumericUpDown();
            this.numSboLiveTime = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.txtProxyIbetAddress = new System.Windows.Forms.TextBox();
            this.txtIpAddress = new System.Windows.Forms.RichTextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ckFakeIpSbo = new System.Windows.Forms.CheckBox();
            this.txtIpFakeSource = new System.Windows.Forms.RichTextBox();
            this.numSboTodayTime = new System.Windows.Forms.NumericUpDown();
            this.numIbetTodayTime = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numIbetLiveTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSboLiveTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSboTodayTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numIbetTodayTime)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(11, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(105, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Login";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Username";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(91, 41);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(179, 20);
            this.textBox1.TabIndex = 2;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(91, 78);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(179, 20);
            this.textBox2.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Password";
            // 
            // txtScanType
            // 
            this.txtScanType.Location = new System.Drawing.Point(91, 105);
            this.txtScanType.Name = "txtScanType";
            this.txtScanType.Size = new System.Drawing.Size(100, 20);
            this.txtScanType.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 109);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Endpoint Type";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Odd Compare";
            // 
            // txtOddCompare
            // 
            this.txtOddCompare.Location = new System.Drawing.Point(92, 141);
            this.txtOddCompare.Name = "txtOddCompare";
            this.txtOddCompare.Size = new System.Drawing.Size(100, 20);
            this.txtOddCompare.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 227);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Live Odd Time IBET";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 257);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Live Odd Time SBO";
            // 
            // numIbetLiveTime
            // 
            this.numIbetLiveTime.Location = new System.Drawing.Point(122, 225);
            this.numIbetLiveTime.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numIbetLiveTime.Minimum = new decimal(new int[] {
            7000,
            0,
            0,
            0});
            this.numIbetLiveTime.Name = "numIbetLiveTime";
            this.numIbetLiveTime.Size = new System.Drawing.Size(120, 20);
            this.numIbetLiveTime.TabIndex = 10;
            this.numIbetLiveTime.Value = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            // 
            // numSboLiveTime
            // 
            this.numSboLiveTime.Location = new System.Drawing.Point(122, 251);
            this.numSboLiveTime.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numSboLiveTime.Minimum = new decimal(new int[] {
            7000,
            0,
            0,
            0});
            this.numSboLiveTime.Name = "numSboLiveTime";
            this.numSboLiveTime.Size = new System.Drawing.Size(120, 20);
            this.numSboLiveTime.TabIndex = 10;
            this.numSboLiveTime.Value = new decimal(new int[] {
            7000,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 175);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(101, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "IBET Proxy Address";
            // 
            // txtProxyIbetAddress
            // 
            this.txtProxyIbetAddress.Location = new System.Drawing.Point(117, 175);
            this.txtProxyIbetAddress.Name = "txtProxyIbetAddress";
            this.txtProxyIbetAddress.Size = new System.Drawing.Size(125, 20);
            this.txtProxyIbetAddress.TabIndex = 8;
            // 
            // txtIpAddress
            // 
            this.txtIpAddress.Location = new System.Drawing.Point(0, 382);
            this.txtIpAddress.Name = "txtIpAddress";
            this.txtIpAddress.Size = new System.Drawing.Size(331, 355);
            this.txtIpAddress.TabIndex = 11;
            this.txtIpAddress.Text = "";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 366);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(58, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "IP Address";
            // 
            // ckFakeIpSbo
            // 
            this.ckFakeIpSbo.AutoSize = true;
            this.ckFakeIpSbo.Location = new System.Drawing.Point(16, 201);
            this.ckFakeIpSbo.Name = "ckFakeIpSbo";
            this.ckFakeIpSbo.Size = new System.Drawing.Size(84, 17);
            this.ckFakeIpSbo.TabIndex = 12;
            this.ckFakeIpSbo.Text = "Fake Ip Sbo";
            this.ckFakeIpSbo.UseVisualStyleBackColor = true;
            // 
            // txtIpFakeSource
            // 
            this.txtIpFakeSource.Dock = System.Windows.Forms.DockStyle.Right;
            this.txtIpFakeSource.Location = new System.Drawing.Point(337, 0);
            this.txtIpFakeSource.Name = "txtIpFakeSource";
            this.txtIpFakeSource.Size = new System.Drawing.Size(143, 737);
            this.txtIpFakeSource.TabIndex = 13;
            this.txtIpFakeSource.Text = resources.GetString("txtIpFakeSource.Text");
            // 
            // numSboTodayTime
            // 
            this.numSboTodayTime.Location = new System.Drawing.Point(162, 312);
            this.numSboTodayTime.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numSboTodayTime.Minimum = new decimal(new int[] {
            7000,
            0,
            0,
            0});
            this.numSboTodayTime.Name = "numSboTodayTime";
            this.numSboTodayTime.Size = new System.Drawing.Size(120, 20);
            this.numSboTodayTime.TabIndex = 16;
            this.numSboTodayTime.Value = new decimal(new int[] {
            7000,
            0,
            0,
            0});
            // 
            // numIbetTodayTime
            // 
            this.numIbetTodayTime.Location = new System.Drawing.Point(162, 286);
            this.numIbetTodayTime.Maximum = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numIbetTodayTime.Minimum = new decimal(new int[] {
            7000,
            0,
            0,
            0});
            this.numIbetTodayTime.Name = "numIbetTodayTime";
            this.numIbetTodayTime.Size = new System.Drawing.Size(120, 20);
            this.numIbetTodayTime.TabIndex = 17;
            this.numIbetTodayTime.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(41, 318);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(111, 13);
            this.label9.TabIndex = 14;
            this.label9.Text = "Today Odd Time SBO";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(42, 288);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(113, 13);
            this.label10.TabIndex = 15;
            this.label10.Text = "Today Odd Time IBET";
            // 
            // frmLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 737);
            this.Controls.Add(this.numSboTodayTime);
            this.Controls.Add(this.numIbetTodayTime);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtIpFakeSource);
            this.Controls.Add(this.ckFakeIpSbo);
            this.Controls.Add(this.txtIpAddress);
            this.Controls.Add(this.numSboLiveTime);
            this.Controls.Add(this.numIbetLiveTime);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtProxyIbetAddress);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtOddCompare);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtScanType);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Name = "frmLogin";
            this.Text = "frmLogin";
            ((System.ComponentModel.ISupportInitialize)(this.numIbetLiveTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSboLiveTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSboTodayTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numIbetTodayTime)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtScanType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtOddCompare;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numIbetLiveTime;
        private System.Windows.Forms.NumericUpDown numSboLiveTime;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtProxyIbetAddress;
        private System.Windows.Forms.RichTextBox txtIpAddress;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox ckFakeIpSbo;
        private System.Windows.Forms.RichTextBox txtIpFakeSource;
        private System.Windows.Forms.NumericUpDown numSboTodayTime;
        private System.Windows.Forms.NumericUpDown numIbetTodayTime;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
    }
}