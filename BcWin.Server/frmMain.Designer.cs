namespace BcWin.Server
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
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabControl = new System.Windows.Forms.TabPage();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button3 = new System.Windows.Forms.Button();
            this.btnLoginSbo = new System.Windows.Forms.Button();
            this.btnLoginIBet = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.tabIbet = new System.Windows.Forms.TabPage();
            this.webIBet = new System.Windows.Forms.WebBrowser();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.webSbo = new System.Windows.Forms.WebBrowser();
            this.tabMain.SuspendLayout();
            this.tabControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabIbet.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabControl);
            this.tabMain.Controls.Add(this.tabIbet);
            this.tabMain.Controls.Add(this.tabPage3);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(1008, 629);
            this.tabMain.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.button6);
            this.tabControl.Controls.Add(this.button5);
            this.tabControl.Controls.Add(this.button4);
            this.tabControl.Controls.Add(this.dataGridView1);
            this.tabControl.Controls.Add(this.button3);
            this.tabControl.Controls.Add(this.btnLoginSbo);
            this.tabControl.Controls.Add(this.btnLoginIBet);
            this.tabControl.Controls.Add(this.button2);
            this.tabControl.Controls.Add(this.button1);
            this.tabControl.Location = new System.Drawing.Point(4, 22);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Windows.Forms.Padding(3);
            this.tabControl.Size = new System.Drawing.Size(1000, 603);
            this.tabControl.TabIndex = 0;
            this.tabControl.Text = "Main";
            this.tabControl.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(8, 311);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(117, 23);
            this.button6.TabIndex = 8;
            this.button6.Text = "FINAL TEST";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(381, 573);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 7;
            this.button5.Text = "update data";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(478, 573);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(108, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "Test Sbo Data";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(161, 6);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(831, 539);
            this.dataGridView1.TabIndex = 5;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(606, 573);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(134, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Test Ibet Data";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnLoginSbo
            // 
            this.btnLoginSbo.Location = new System.Drawing.Point(8, 99);
            this.btnLoginSbo.Name = "btnLoginSbo";
            this.btnLoginSbo.Size = new System.Drawing.Size(75, 23);
            this.btnLoginSbo.TabIndex = 3;
            this.btnLoginSbo.Text = "Login Sbo";
            this.btnLoginSbo.UseVisualStyleBackColor = true;
            this.btnLoginSbo.Click += new System.EventHandler(this.btnLoginSbo_Click);
            // 
            // btnLoginIBet
            // 
            this.btnLoginIBet.Location = new System.Drawing.Point(8, 70);
            this.btnLoginIBet.Name = "btnLoginIBet";
            this.btnLoginIBet.Size = new System.Drawing.Size(75, 23);
            this.btnLoginIBet.TabIndex = 2;
            this.btnLoginIBet.Text = "Login IBet";
            this.btnLoginIBet.UseVisualStyleBackColor = true;
            this.btnLoginIBet.Click += new System.EventHandler(this.btnLoginIBet_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(8, 128);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Close ALL";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(763, 573);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "btn1 Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabIbet
            // 
            this.tabIbet.Controls.Add(this.webIBet);
            this.tabIbet.Location = new System.Drawing.Point(4, 22);
            this.tabIbet.Name = "tabIbet";
            this.tabIbet.Padding = new System.Windows.Forms.Padding(3);
            this.tabIbet.Size = new System.Drawing.Size(1000, 603);
            this.tabIbet.TabIndex = 1;
            this.tabIbet.Text = "IBet";
            this.tabIbet.UseVisualStyleBackColor = true;
            // 
            // webIBet
            // 
            this.webIBet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webIBet.Location = new System.Drawing.Point(3, 3);
            this.webIBet.MinimumSize = new System.Drawing.Size(20, 20);
            this.webIBet.Name = "webIBet";
            this.webIBet.Size = new System.Drawing.Size(994, 597);
            this.webIBet.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.webSbo);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1000, 603);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "SBO";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // webSbo
            // 
            this.webSbo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webSbo.Location = new System.Drawing.Point(3, 3);
            this.webSbo.MinimumSize = new System.Drawing.Size(20, 20);
            this.webSbo.Name = "webSbo";
            this.webSbo.Size = new System.Drawing.Size(994, 597);
            this.webSbo.TabIndex = 0;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 629);
            this.Controls.Add(this.tabMain);
            this.Name = "frmMain";
            this.Text = "Form1";
            this.tabMain.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabIbet.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabControl;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TabPage tabIbet;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.WebBrowser webIBet;
        private System.Windows.Forms.WebBrowser webSbo;
        private System.Windows.Forms.Button btnLoginSbo;
        private System.Windows.Forms.Button btnLoginIBet;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
    }
}

