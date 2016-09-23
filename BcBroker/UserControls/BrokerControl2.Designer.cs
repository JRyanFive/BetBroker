namespace BcBroker.UserControls
{
    partial class BrokerControl2
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrokerControl));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.cbOddPairCheck = new System.Windows.Forms.ComboBox();
            this.cbGoalDefCheck = new System.Windows.Forms.ComboBox();
            this.numTimeCheckScan = new System.Windows.Forms.NumericUpDown();
            this.txtStake = new System.Windows.Forms.TextBox();
            this.lbSell = new System.Windows.Forms.Label();
            this.lbMissTrans = new System.Windows.Forms.Label();
            this.lbBuy = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.groupAccount = new DevExpress.XtraEditors.GroupControl();
            this.btnAddAcc = new DevExpress.XtraEditors.SimpleButton();
            this.dgvAccount = new System.Windows.Forms.DataGridView();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Password = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Rate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Login = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewLinkColumn();
            this.Edit = new System.Windows.Forms.DataGridViewLinkColumn();
            this.Delete = new System.Windows.Forms.DataGridViewLinkColumn();
            this.Domain = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EngineId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MainRate = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnStop = new System.Windows.Forms.PictureBox();
            this.btnPause = new System.Windows.Forms.PictureBox();
            this.btnStart = new System.Windows.Forms.PictureBox();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
            this.btnClearLog = new DevExpress.XtraEditors.SimpleButton();
            this.label4 = new System.Windows.Forms.Label();
            this.cbOddDev = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
            this.txtIpProxyIbet = new System.Windows.Forms.TextBox();
            this.ckProxyIbet = new System.Windows.Forms.CheckBox();
            this.ckRandomIpIbet = new System.Windows.Forms.CheckBox();
            this.ckRandomIpSbo = new System.Windows.Forms.CheckBox();
            this.xtraTabPage3 = new DevExpress.XtraTab.XtraTabPage();
            this.ckAllLeague = new System.Windows.Forms.CheckBox();
            this.dgvLeaguesSetting = new System.Windows.Forms.DataGridView();
            this.Selected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.LeagueName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SboName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LeagueValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.xtraTabControl2 = new DevExpress.XtraTab.XtraTabControl();
            this.xtraTabPage4 = new DevExpress.XtraTab.XtraTabPage();
            this.xtraTabPage5 = new DevExpress.XtraTab.XtraTabPage();
            this.txtLogBet = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeCheckScan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupAccount)).BeginInit();
            this.groupAccount.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAccount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnStop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPause)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.xtraTabControl1.SuspendLayout();
            this.xtraTabPage1.SuspendLayout();
            this.xtraTabPage2.SuspendLayout();
            this.xtraTabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLeaguesSetting)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl2)).BeginInit();
            this.xtraTabControl2.SuspendLayout();
            this.xtraTabPage4.SuspendLayout();
            this.xtraTabPage5.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbOddPairCheck
            // 
            this.cbOddPairCheck.FormattingEnabled = true;
            this.cbOddPairCheck.Items.AddRange(new object[] {
            "0",
            "0.05",
            "0.1",
            "0.15",
            "0.2"});
            this.cbOddPairCheck.Location = new System.Drawing.Point(231, 159);
            this.cbOddPairCheck.Name = "cbOddPairCheck";
            this.cbOddPairCheck.Size = new System.Drawing.Size(50, 21);
            this.cbOddPairCheck.TabIndex = 63;
            // 
            // cbGoalDefCheck
            // 
            this.cbGoalDefCheck.FormattingEnabled = true;
            this.cbGoalDefCheck.Items.AddRange(new object[] {
            "0",
            "0.25",
            "0.5",
            "0.75",
            "1",
            "1.25",
            "1.5",
            "1.75",
            "2"});
            this.cbGoalDefCheck.Location = new System.Drawing.Point(131, 127);
            this.cbGoalDefCheck.Name = "cbGoalDefCheck";
            this.cbGoalDefCheck.Size = new System.Drawing.Size(54, 21);
            this.cbGoalDefCheck.TabIndex = 62;
            // 
            // numTimeCheckScan
            // 
            this.numTimeCheckScan.Location = new System.Drawing.Point(77, 159);
            this.numTimeCheckScan.Name = "numTimeCheckScan";
            this.numTimeCheckScan.Size = new System.Drawing.Size(48, 20);
            this.numTimeCheckScan.TabIndex = 61;
            this.numTimeCheckScan.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // txtStake
            // 
            this.txtStake.Location = new System.Drawing.Point(92, 97);
            this.txtStake.Name = "txtStake";
            this.txtStake.Size = new System.Drawing.Size(133, 20);
            this.txtStake.TabIndex = 59;
            // 
            // lbSell
            // 
            this.lbSell.AutoSize = true;
            this.lbSell.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSell.ForeColor = System.Drawing.Color.Red;
            this.lbSell.Location = new System.Drawing.Point(373, 190);
            this.lbSell.Name = "lbSell";
            this.lbSell.Size = new System.Drawing.Size(16, 18);
            this.lbSell.TabIndex = 55;
            this.lbSell.Text = "0";
            this.lbSell.Visible = false;
            // 
            // lbMissTrans
            // 
            this.lbMissTrans.AutoSize = true;
            this.lbMissTrans.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMissTrans.ForeColor = System.Drawing.Color.Black;
            this.lbMissTrans.Location = new System.Drawing.Point(450, 221);
            this.lbMissTrans.Name = "lbMissTrans";
            this.lbMissTrans.Size = new System.Drawing.Size(15, 16);
            this.lbMissTrans.TabIndex = 54;
            this.lbMissTrans.Text = "0";
            this.lbMissTrans.Visible = false;
            // 
            // lbBuy
            // 
            this.lbBuy.AutoSize = true;
            this.lbBuy.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbBuy.ForeColor = System.Drawing.Color.Red;
            this.lbBuy.Location = new System.Drawing.Point(373, 158);
            this.lbBuy.Name = "lbBuy";
            this.lbBuy.Size = new System.Drawing.Size(16, 18);
            this.lbBuy.TabIndex = 56;
            this.lbBuy.Text = "0";
            this.lbBuy.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.lblStatus.Location = new System.Drawing.Point(268, 69);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(61, 17);
            this.lblStatus.TabIndex = 45;
            this.lblStatus.Text = "Running";
            this.lblStatus.Visible = false;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.ForeColor = System.Drawing.Color.Navy;
            this.label18.Location = new System.Drawing.Point(326, 191);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(39, 16);
            this.label18.TabIndex = 50;
            this.label18.Text = "BÁN:";
            this.label18.Visible = false;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label24.ForeColor = System.Drawing.Color.Navy;
            this.label24.Location = new System.Drawing.Point(326, 221);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(118, 16);
            this.label24.TabIndex = 52;
            this.label24.Text = "Không bán được:";
            this.label24.Visible = false;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Navy;
            this.label17.Location = new System.Drawing.Point(326, 161);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(41, 16);
            this.label17.TabIndex = 49;
            this.label17.Text = "MUA:";
            this.label17.Visible = false;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(541, 422);
            this.txtLog.TabIndex = 36;
            this.txtLog.Text = "";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(287, 162);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(21, 13);
            this.label28.TabIndex = 42;
            this.label28.Text = "giá";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(3, 162);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(68, 13);
            this.label14.TabIndex = 41;
            this.label14.Text = "Bán liền sau:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(3, 134);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(122, 13);
            this.label13.TabIndex = 40;
            this.label13.Text = "Kèo HDP chấp tối thiểu:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(131, 162);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(94, 13);
            this.label16.TabIndex = 39;
            this.label16.Text = "phút. Lời tối thiểu :";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 100);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(84, 13);
            this.label11.TabIndex = 37;
            this.label11.Text = "Tổng điểm mua:";
            // 
            // groupAccount
            // 
            this.groupAccount.CaptionImage = ((System.Drawing.Image)(resources.GetObject("groupAccount.CaptionImage")));
            this.groupAccount.Controls.Add(this.btnAddAcc);
            this.groupAccount.Controls.Add(this.dgvAccount);
            this.groupAccount.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupAccount.Location = new System.Drawing.Point(0, 0);
            this.groupAccount.Name = "groupAccount";
            this.groupAccount.Size = new System.Drawing.Size(391, 726);
            this.groupAccount.TabIndex = 66;
            this.groupAccount.Text = "CẤU HÌNH TÀI KHOẢN";
            this.groupAccount.MouseClick += new System.Windows.Forms.MouseEventHandler(this.EventGroupAccClick);
            // 
            // btnAddAcc
            // 
            this.btnAddAcc.Image = ((System.Drawing.Image)(resources.GetObject("btnAddAcc.Image")));
            this.btnAddAcc.Location = new System.Drawing.Point(347, 0);
            this.btnAddAcc.Name = "btnAddAcc";
            this.btnAddAcc.Size = new System.Drawing.Size(42, 40);
            this.btnAddAcc.TabIndex = 66;
            this.btnAddAcc.Text = "simpleButton1";
            this.btnAddAcc.Click += new System.EventHandler(this.btnAddAcc_Click);
            // 
            // dgvAccount
            // 
            this.dgvAccount.AllowUserToAddRows = false;
            this.dgvAccount.AllowUserToDeleteRows = false;
            this.dgvAccount.AllowUserToResizeRows = false;
            this.dgvAccount.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAccount.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvAccount.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAccount.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Type,
            this.Column2,
            this.Password,
            this.Rate,
            this.Login,
            this.Column6,
            this.Edit,
            this.Delete,
            this.Domain,
            this.EngineId,
            this.MainRate});
            this.dgvAccount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAccount.Location = new System.Drawing.Point(2, 40);
            this.dgvAccount.Name = "dgvAccount";
            this.dgvAccount.RowHeadersVisible = false;
            this.dgvAccount.Size = new System.Drawing.Size(387, 684);
            this.dgvAccount.TabIndex = 65;
            this.dgvAccount.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAccount_CellClick);
            // 
            // Type
            // 
            this.Type.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Type.HeaderText = "";
            this.Type.Name = "Type";
            this.Type.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Type.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Type.Width = 5;
            // 
            // Column2
            // 
            this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column2.HeaderText = "Username";
            this.Column2.Name = "Column2";
            this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column2.Width = 59;
            // 
            // Password
            // 
            this.Password.HeaderText = "Password";
            this.Password.Name = "Password";
            // 
            // Rate
            // 
            this.Rate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Rate.HeaderText = "Rate";
            this.Rate.Name = "Rate";
            this.Rate.Width = 53;
            // 
            // Login
            // 
            this.Login.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Login.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Login.HeaderText = "";
            this.Login.Name = "Login";
            this.Login.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Login.Text = "Login";
            this.Login.Width = 5;
            // 
            // Column6
            // 
            this.Column6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column6.HeaderText = "";
            this.Column6.Name = "Column6";
            this.Column6.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column6.Text = "$";
            this.Column6.UseColumnTextForLinkValue = true;
            this.Column6.VisitedLinkColor = System.Drawing.Color.Blue;
            this.Column6.Width = 5;
            // 
            // Edit
            // 
            this.Edit.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Edit.HeaderText = "";
            this.Edit.Name = "Edit";
            this.Edit.Text = "E";
            this.Edit.UseColumnTextForLinkValue = true;
            this.Edit.VisitedLinkColor = System.Drawing.Color.Blue;
            this.Edit.Width = 5;
            // 
            // Delete
            // 
            this.Delete.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Delete.HeaderText = "";
            this.Delete.Name = "Delete";
            this.Delete.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Delete.Text = "x";
            this.Delete.UseColumnTextForLinkValue = true;
            this.Delete.VisitedLinkColor = System.Drawing.Color.Blue;
            this.Delete.Width = 5;
            // 
            // Domain
            // 
            this.Domain.HeaderText = "Domain";
            this.Domain.Name = "Domain";
            this.Domain.Visible = false;
            // 
            // EngineId
            // 
            this.EngineId.HeaderText = "EngineId";
            this.EngineId.Name = "EngineId";
            this.EngineId.Visible = false;
            // 
            // MainRate
            // 
            this.MainRate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.MainRate.HeaderText = "";
            this.MainRate.Name = "MainRate";
            this.MainRate.Width = 5;
            // 
            // btnStop
            // 
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.Image = global::BcBroker.Properties.Resources.StopBtn;
            this.btnStop.Location = new System.Drawing.Point(316, 15);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(40, 40);
            this.btnStop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.btnStop.TabIndex = 53;
            this.btnStop.TabStop = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnPause
            // 
            this.btnPause.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPause.Image = global::BcBroker.Properties.Resources.PauseBtn;
            this.btnPause.Location = new System.Drawing.Point(238, 15);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(40, 40);
            this.btnPause.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.btnPause.TabIndex = 48;
            this.btnPause.TabStop = false;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnStart
            // 
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.Image = global::BcBroker.Properties.Resources.PlayBtn;
            this.btnStart.Location = new System.Drawing.Point(159, 15);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(40, 40);
            this.btnStart.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.btnStart.TabIndex = 46;
            this.btnStart.TabStop = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // pbLoading
            // 
            this.pbLoading.Image = global::BcBroker.Properties.Resources.ProcessBar;
            this.pbLoading.Location = new System.Drawing.Point(159, 61);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(280, 5);
            this.pbLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbLoading.TabIndex = 44;
            this.pbLoading.TabStop = false;
            this.pbLoading.Visible = false;
            // 
            // btnSave
            // 
            this.btnSave.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.Location = new System.Drawing.Point(399, 15);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(40, 40);
            this.btnSave.TabIndex = 68;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xtraTabControl1.Location = new System.Drawing.Point(395, 3);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.SelectedTabPage = this.xtraTabPage1;
            this.xtraTabControl1.Size = new System.Drawing.Size(547, 272);
            this.xtraTabControl1.TabIndex = 69;
            this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPage1,
            this.xtraTabPage2,
            this.xtraTabPage3});
            // 
            // xtraTabPage1
            // 
            this.xtraTabPage1.Controls.Add(this.btnClearLog);
            this.xtraTabPage1.Controls.Add(this.btnStart);
            this.xtraTabPage1.Controls.Add(this.btnSave);
            this.xtraTabPage1.Controls.Add(this.label11);
            this.xtraTabPage1.Controls.Add(this.label4);
            this.xtraTabPage1.Controls.Add(this.label16);
            this.xtraTabPage1.Controls.Add(this.label13);
            this.xtraTabPage1.Controls.Add(this.cbOddDev);
            this.xtraTabPage1.Controls.Add(this.cbOddPairCheck);
            this.xtraTabPage1.Controls.Add(this.label14);
            this.xtraTabPage1.Controls.Add(this.cbGoalDefCheck);
            this.xtraTabPage1.Controls.Add(this.label3);
            this.xtraTabPage1.Controls.Add(this.label28);
            this.xtraTabPage1.Controls.Add(this.numTimeCheckScan);
            this.xtraTabPage1.Controls.Add(this.label17);
            this.xtraTabPage1.Controls.Add(this.txtStake);
            this.xtraTabPage1.Controls.Add(this.label24);
            this.xtraTabPage1.Controls.Add(this.btnStop);
            this.xtraTabPage1.Controls.Add(this.label18);
            this.xtraTabPage1.Controls.Add(this.btnPause);
            this.xtraTabPage1.Controls.Add(this.pbLoading);
            this.xtraTabPage1.Controls.Add(this.lblStatus);
            this.xtraTabPage1.Controls.Add(this.lbSell);
            this.xtraTabPage1.Controls.Add(this.lbBuy);
            this.xtraTabPage1.Controls.Add(this.lbMissTrans);
            this.xtraTabPage1.Name = "xtraTabPage1";
            this.xtraTabPage1.Size = new System.Drawing.Size(541, 244);
            this.xtraTabPage1.Text = "Thông tin";
            // 
            // btnClearLog
            // 
            this.btnClearLog.Image = ((System.Drawing.Image)(resources.GetObject("btnClearLog.Image")));
            this.btnClearLog.Location = new System.Drawing.Point(6, 218);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(78, 23);
            this.btnClearLog.TabIndex = 71;
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(204, 134);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 13);
            this.label4.TabIndex = 39;
            this.label4.Text = "Lệch tối đa:";
            // 
            // cbOddDev
            // 
            this.cbOddDev.FormattingEnabled = true;
            this.cbOddDev.Items.AddRange(new object[] {
            "10",
            "15",
            "20"});
            this.cbOddDev.Location = new System.Drawing.Point(269, 127);
            this.cbOddDev.Name = "cbOddDev";
            this.cbOddDev.Size = new System.Drawing.Size(50, 21);
            this.cbOddDev.TabIndex = 63;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(325, 130);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(18, 13);
            this.label3.TabIndex = 42;
            this.label3.Text = "xu";
            // 
            // xtraTabPage2
            // 
            this.xtraTabPage2.Controls.Add(this.txtIpProxyIbet);
            this.xtraTabPage2.Controls.Add(this.ckProxyIbet);
            this.xtraTabPage2.Controls.Add(this.ckRandomIpIbet);
            this.xtraTabPage2.Controls.Add(this.ckRandomIpSbo);
            this.xtraTabPage2.Name = "xtraTabPage2";
            this.xtraTabPage2.Size = new System.Drawing.Size(541, 244);
            this.xtraTabPage2.Text = "Cấu hình";
            // 
            // txtIpProxyIbet
            // 
            this.txtIpProxyIbet.Enabled = false;
            this.txtIpProxyIbet.Location = new System.Drawing.Point(117, 64);
            this.txtIpProxyIbet.Name = "txtIpProxyIbet";
            this.txtIpProxyIbet.Size = new System.Drawing.Size(158, 20);
            this.txtIpProxyIbet.TabIndex = 2;
            // 
            // ckProxyIbet
            // 
            this.ckProxyIbet.AutoSize = true;
            this.ckProxyIbet.Location = new System.Drawing.Point(14, 66);
            this.ckProxyIbet.Name = "ckProxyIbet";
            this.ckProxyIbet.Size = new System.Drawing.Size(97, 17);
            this.ckProxyIbet.TabIndex = 0;
            this.ckProxyIbet.Text = "Proxy login ibet";
            this.ckProxyIbet.UseVisualStyleBackColor = true;
            this.ckProxyIbet.CheckedChanged += new System.EventHandler(this.ckProxyIbet_CheckedChanged);
            // 
            // ckRandomIpIbet
            // 
            this.ckRandomIpIbet.AutoSize = true;
            this.ckRandomIpIbet.Enabled = false;
            this.ckRandomIpIbet.Location = new System.Drawing.Point(14, 40);
            this.ckRandomIpIbet.Name = "ckRandomIpIbet";
            this.ckRandomIpIbet.Size = new System.Drawing.Size(169, 17);
            this.ckRandomIpIbet.TabIndex = 0;
            this.ckRandomIpIbet.Text = "Fake random ip tài khoản ibet.";
            this.ckRandomIpIbet.UseVisualStyleBackColor = true;
            // 
            // ckRandomIpSbo
            // 
            this.ckRandomIpSbo.AutoSize = true;
            this.ckRandomIpSbo.Location = new System.Drawing.Point(14, 14);
            this.ckRandomIpSbo.Name = "ckRandomIpSbo";
            this.ckRandomIpSbo.Size = new System.Drawing.Size(169, 17);
            this.ckRandomIpSbo.TabIndex = 0;
            this.ckRandomIpSbo.Text = "Fake random ip tài khoản sbo.";
            this.ckRandomIpSbo.UseVisualStyleBackColor = true;
            // 
            // xtraTabPage3
            // 
            this.xtraTabPage3.Controls.Add(this.ckAllLeague);
            this.xtraTabPage3.Controls.Add(this.dgvLeaguesSetting);
            this.xtraTabPage3.Name = "xtraTabPage3";
            this.xtraTabPage3.Size = new System.Drawing.Size(541, 244);
            this.xtraTabPage3.Text = "Giải đấu";
            // 
            // ckAllLeague
            // 
            this.ckAllLeague.AutoSize = true;
            this.ckAllLeague.Enabled = false;
            this.ckAllLeague.Location = new System.Drawing.Point(6, 5);
            this.ckAllLeague.Name = "ckAllLeague";
            this.ckAllLeague.Size = new System.Drawing.Size(81, 17);
            this.ckAllLeague.TabIndex = 2;
            this.ckAllLeague.Text = "Chon tất cả";
            this.ckAllLeague.UseVisualStyleBackColor = true;
            this.ckAllLeague.CheckedChanged += new System.EventHandler(this.ckAllLeague_CheckedChanged);
            // 
            // dgvLeaguesSetting
            // 
            this.dgvLeaguesSetting.AllowUserToAddRows = false;
            this.dgvLeaguesSetting.AllowUserToDeleteRows = false;
            this.dgvLeaguesSetting.AllowUserToOrderColumns = true;
            this.dgvLeaguesSetting.AllowUserToResizeRows = false;
            this.dgvLeaguesSetting.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLeaguesSetting.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLeaguesSetting.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Selected,
            this.LeagueName,
            this.SboName,
            this.LeagueValue});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(31)))), ((int)(((byte)(53)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvLeaguesSetting.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvLeaguesSetting.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvLeaguesSetting.Enabled = false;
            this.dgvLeaguesSetting.GridColor = System.Drawing.SystemColors.Control;
            this.dgvLeaguesSetting.Location = new System.Drawing.Point(0, 28);
            this.dgvLeaguesSetting.MultiSelect = false;
            this.dgvLeaguesSetting.Name = "dgvLeaguesSetting";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ActiveCaption;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.MediumBlue;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvLeaguesSetting.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvLeaguesSetting.RowHeadersVisible = false;
            this.dgvLeaguesSetting.Size = new System.Drawing.Size(541, 216);
            this.dgvLeaguesSetting.TabIndex = 1;
            // 
            // Selected
            // 
            this.Selected.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Selected.DataPropertyName = "Selected";
            this.Selected.HeaderText = "";
            this.Selected.Name = "Selected";
            this.Selected.Width = 5;
            // 
            // LeagueName
            // 
            this.LeagueName.DataPropertyName = "IbetLeagueName";
            this.LeagueName.HeaderText = "Giải đấu";
            this.LeagueName.Name = "LeagueName";
            // 
            // SboName
            // 
            this.SboName.DataPropertyName = "SboLeagueName";
            this.SboName.HeaderText = "SboName";
            this.SboName.Name = "SboName";
            this.SboName.Visible = false;
            // 
            // LeagueValue
            // 
            this.LeagueValue.DataPropertyName = "LeagueValue";
            this.LeagueValue.HeaderText = "LeagueValue";
            this.LeagueValue.Name = "LeagueValue";
            this.LeagueValue.Visible = false;
            // 
            // xtraTabControl2
            // 
            this.xtraTabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xtraTabControl2.Location = new System.Drawing.Point(395, 276);
            this.xtraTabControl2.Name = "xtraTabControl2";
            this.xtraTabControl2.SelectedTabPage = this.xtraTabPage4;
            this.xtraTabControl2.Size = new System.Drawing.Size(547, 450);
            this.xtraTabControl2.TabIndex = 70;
            this.xtraTabControl2.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPage4,
            this.xtraTabPage5});
            // 
            // xtraTabPage4
            // 
            this.xtraTabPage4.Controls.Add(this.txtLog);
            this.xtraTabPage4.Name = "xtraTabPage4";
            this.xtraTabPage4.Size = new System.Drawing.Size(541, 422);
            this.xtraTabPage4.Text = "Log Scan";
            // 
            // xtraTabPage5
            // 
            this.xtraTabPage5.Controls.Add(this.txtLogBet);
            this.xtraTabPage5.Name = "xtraTabPage5";
            this.xtraTabPage5.Size = new System.Drawing.Size(541, 422);
            this.xtraTabPage5.Text = "Log Bet";
            // 
            // txtLogBet
            // 
            this.txtLogBet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLogBet.Location = new System.Drawing.Point(0, 0);
            this.txtLogBet.Name = "txtLogBet";
            this.txtLogBet.ReadOnly = true;
            this.txtLogBet.Size = new System.Drawing.Size(541, 422);
            this.txtLogBet.TabIndex = 0;
            this.txtLogBet.Text = "";
            // 
            // BrokerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.xtraTabControl2);
            this.Controls.Add(this.xtraTabControl1);
            this.Controls.Add(this.groupAccount);
            this.Name = "BrokerControl";
            this.Size = new System.Drawing.Size(942, 726);
            ((System.ComponentModel.ISupportInitialize)(this.numTimeCheckScan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupAccount)).EndInit();
            this.groupAccount.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAccount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnStop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPause)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            this.xtraTabControl1.ResumeLayout(false);
            this.xtraTabPage1.ResumeLayout(false);
            this.xtraTabPage1.PerformLayout();
            this.xtraTabPage2.ResumeLayout(false);
            this.xtraTabPage2.PerformLayout();
            this.xtraTabPage3.ResumeLayout(false);
            this.xtraTabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLeaguesSetting)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl2)).EndInit();
            this.xtraTabControl2.ResumeLayout(false);
            this.xtraTabPage4.ResumeLayout(false);
            this.xtraTabPage5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbOddPairCheck;
        private System.Windows.Forms.ComboBox cbGoalDefCheck;
        private System.Windows.Forms.NumericUpDown numTimeCheckScan;
        private System.Windows.Forms.TextBox txtStake;
        private System.Windows.Forms.PictureBox btnStop;
        private System.Windows.Forms.PictureBox btnPause;
        private System.Windows.Forms.PictureBox btnStart;
        private System.Windows.Forms.Label lbSell;
        private System.Windows.Forms.Label lbMissTrans;
        private System.Windows.Forms.Label lbBuy;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.PictureBox pbLoading;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.DataGridView dgvAccount;
        private DevExpress.XtraEditors.GroupControl groupAccount;
        private DevExpress.XtraEditors.SimpleButton btnAddAcc;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage2;
        private System.Windows.Forms.CheckBox ckRandomIpIbet;
        private System.Windows.Forms.CheckBox ckRandomIpSbo;
        private System.Windows.Forms.TextBox txtIpProxyIbet;
        private System.Windows.Forms.CheckBox ckProxyIbet;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage3;
        private System.Windows.Forms.CheckBox ckAllLeague;
        private System.Windows.Forms.DataGridView dgvLeaguesSetting;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Selected;
        private System.Windows.Forms.DataGridViewTextBoxColumn LeagueName;
        private System.Windows.Forms.DataGridViewTextBoxColumn SboName;
        private System.Windows.Forms.DataGridViewTextBoxColumn LeagueValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbOddDev;
        private System.Windows.Forms.Label label3;
        private DevExpress.XtraEditors.SimpleButton btnClearLog;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Password;
        private System.Windows.Forms.DataGridViewTextBoxColumn Rate;
        private System.Windows.Forms.DataGridViewButtonColumn Login;
        private System.Windows.Forms.DataGridViewLinkColumn Column6;
        private System.Windows.Forms.DataGridViewLinkColumn Edit;
        private System.Windows.Forms.DataGridViewLinkColumn Delete;
        private System.Windows.Forms.DataGridViewTextBoxColumn Domain;
        private System.Windows.Forms.DataGridViewTextBoxColumn EngineId;
        private System.Windows.Forms.DataGridViewCheckBoxColumn MainRate;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl2;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage4;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage5;
        private System.Windows.Forms.RichTextBox txtLogBet;
    }
}
