namespace BcBetRun
{
    partial class frmSboBet
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSboBet));
            this.btnStart = new System.Windows.Forms.Button();
            this.numMaxStake = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numSellAfterSecond = new System.Windows.Forms.NumericUpDown();
            this.ckSellAfter = new System.Windows.Forms.CheckBox();
            this.txtStake = new System.Windows.Forms.TextBox();
            this.dgvLogScan = new System.Windows.Forms.DataGridView();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Home = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Away = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Odd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rdBetAway = new System.Windows.Forms.RadioButton();
            this.rdBetHome = new System.Windows.Forms.RadioButton();
            this.btnTrade = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSellEx = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBuyEx = new System.Windows.Forms.TextBox();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.dgvUserBuy = new System.Windows.Forms.DataGridView();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Password = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Login = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewLinkColumn();
            this.dgvUserSell = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewButtonColumn1 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewLinkColumn();
            this.cboMarket = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtUserscan = new System.Windows.Forms.TextBox();
            this.txtPassScan = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSaveExchange = new System.Windows.Forms.PictureBox();
            this.btnSaveExchange2 = new System.Windows.Forms.PictureBox();
            this.btnLoginBuyGroup = new System.Windows.Forms.PictureBox();
            this.btnLoginSellGroup = new System.Windows.Forms.PictureBox();
            this.ckBuyAccRandomIp = new System.Windows.Forms.CheckBox();
            this.ckSellAccRandomIp = new System.Windows.Forms.CheckBox();
            this.txtIpFakeScan = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxStake)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSellAfterSecond)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLogScan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserBuy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserSell)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSaveExchange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSaveExchange2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnLoginBuyGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnLoginSellGroup)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(4, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(157, 47);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Bắt đầu";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // numMaxStake
            // 
            this.numMaxStake.Location = new System.Drawing.Point(458, 37);
            this.numMaxStake.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numMaxStake.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numMaxStake.Name = "numMaxStake";
            this.numMaxStake.Size = new System.Drawing.Size(74, 20);
            this.numMaxStake.TabIndex = 5;
            this.numMaxStake.Value = new decimal(new int[] {
            1500,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Số tiền mua:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numSellAfterSecond);
            this.groupBox1.Controls.Add(this.ckSellAfter);
            this.groupBox1.Controls.Add(this.txtStake);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.dgvLogScan);
            this.groupBox1.Controls.Add(this.rdBetAway);
            this.groupBox1.Controls.Add(this.rdBetHome);
            this.groupBox1.Controls.Add(this.btnTrade);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(538, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(551, 542);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Giao dịch Mua/Bán";
            // 
            // numSellAfterSecond
            // 
            this.numSellAfterSecond.Location = new System.Drawing.Point(196, 45);
            this.numSellAfterSecond.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numSellAfterSecond.Name = "numSellAfterSecond";
            this.numSellAfterSecond.Size = new System.Drawing.Size(56, 20);
            this.numSellAfterSecond.TabIndex = 10;
            // 
            // ckSellAfter
            // 
            this.ckSellAfter.AutoSize = true;
            this.ckSellAfter.Location = new System.Drawing.Point(14, 45);
            this.ckSellAfter.Name = "ckSellAfter";
            this.ckSellAfter.Size = new System.Drawing.Size(179, 17);
            this.ckSellAfter.TabIndex = 9;
            this.ckSellAfter.Text = "Đánh tổng tiền, bán liền sau sau";
            this.ckSellAfter.UseVisualStyleBackColor = true;
            // 
            // txtStake
            // 
            this.txtStake.Location = new System.Drawing.Point(80, 19);
            this.txtStake.Name = "txtStake";
            this.txtStake.Size = new System.Drawing.Size(130, 20);
            this.txtStake.TabIndex = 8;
            // 
            // dgvLogScan
            // 
            this.dgvLogScan.AllowUserToAddRows = false;
            this.dgvLogScan.AllowUserToDeleteRows = false;
            this.dgvLogScan.AllowUserToResizeRows = false;
            this.dgvLogScan.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLogScan.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLogScan.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column3,
            this.Home,
            this.Away,
            this.Type,
            this.Odd,
            this.Column1,
            this.Column4,
            this.Column5});
            this.dgvLogScan.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvLogScan.Location = new System.Drawing.Point(3, 73);
            this.dgvLogScan.MultiSelect = false;
            this.dgvLogScan.Name = "dgvLogScan";
            this.dgvLogScan.ReadOnly = true;
            this.dgvLogScan.RowHeadersVisible = false;
            this.dgvLogScan.Size = new System.Drawing.Size(545, 466);
            this.dgvLogScan.TabIndex = 7;
            // 
            // Column3
            // 
            this.Column3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column3.HeaderText = "";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column3.Width = 5;
            // 
            // Home
            // 
            this.Home.HeaderText = "Home Team";
            this.Home.Name = "Home";
            this.Home.ReadOnly = true;
            this.Home.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Away
            // 
            this.Away.HeaderText = "Away Team";
            this.Away.Name = "Away";
            this.Away.ReadOnly = true;
            this.Away.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Type
            // 
            this.Type.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Type.HeaderText = "Type";
            this.Type.Name = "Type";
            this.Type.ReadOnly = true;
            this.Type.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Type.Width = 37;
            // 
            // Odd
            // 
            this.Odd.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Odd.HeaderText = "Odd";
            this.Odd.Name = "Odd";
            this.Odd.ReadOnly = true;
            this.Odd.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Odd.Width = 33;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column1.HeaderText = "H";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column1.Width = 21;
            // 
            // Column4
            // 
            this.Column4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column4.HeaderText = "A";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column4.Width = 20;
            // 
            // Column5
            // 
            this.Column5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Column5.DefaultCellStyle = dataGridViewCellStyle1;
            this.Column5.HeaderText = "Max";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            this.Column5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column5.Width = 33;
            // 
            // rdBetAway
            // 
            this.rdBetAway.AutoSize = true;
            this.rdBetAway.Location = new System.Drawing.Point(301, 20);
            this.rdBetAway.Name = "rdBetAway";
            this.rdBetAway.Size = new System.Drawing.Size(51, 17);
            this.rdBetAway.TabIndex = 0;
            this.rdBetAway.Text = "Away";
            this.rdBetAway.UseVisualStyleBackColor = true;
            this.rdBetAway.CheckedChanged += new System.EventHandler(this.rdBetAway_CheckedChanged);
            // 
            // rdBetHome
            // 
            this.rdBetHome.AutoSize = true;
            this.rdBetHome.Checked = true;
            this.rdBetHome.Location = new System.Drawing.Point(231, 20);
            this.rdBetHome.Name = "rdBetHome";
            this.rdBetHome.Size = new System.Drawing.Size(53, 17);
            this.rdBetHome.TabIndex = 0;
            this.rdBetHome.TabStop = true;
            this.rdBetHome.Text = "Home";
            this.rdBetHome.UseVisualStyleBackColor = true;
            this.rdBetHome.CheckedChanged += new System.EventHandler(this.rdBetHome_CheckedChanged);
            // 
            // btnTrade
            // 
            this.btnTrade.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTrade.Location = new System.Drawing.Point(419, 16);
            this.btnTrade.Name = "btnTrade";
            this.btnTrade.Size = new System.Drawing.Size(100, 30);
            this.btnTrade.TabIndex = 1;
            this.btnTrade.Text = "TRADE";
            this.btnTrade.UseVisualStyleBackColor = true;
            this.btnTrade.Click += new System.EventHandler(this.btnTrade_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(258, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "mili giây";
            // 
            // txtSellEx
            // 
            this.txtSellEx.Location = new System.Drawing.Point(370, 65);
            this.txtSellEx.Name = "txtSellEx";
            this.txtSellEx.Size = new System.Drawing.Size(135, 20);
            this.txtSellEx.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(305, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Tỷ giá bán:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(32, 69);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Tỷ giá mua:";
            // 
            // txtBuyEx
            // 
            this.txtBuyEx.Location = new System.Drawing.Point(100, 66);
            this.txtBuyEx.Name = "txtBuyEx";
            this.txtBuyEx.Size = new System.Drawing.Size(134, 20);
            this.txtBuyEx.TabIndex = 9;
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(538, 551);
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(556, 182);
            this.txtLog.TabIndex = 10;
            this.txtLog.Text = "";
            // 
            // dgvUserBuy
            // 
            this.dgvUserBuy.AllowUserToDeleteRows = false;
            this.dgvUserBuy.AllowUserToResizeRows = false;
            this.dgvUserBuy.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUserBuy.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUserBuy.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column2,
            this.Password,
            this.Login,
            this.Column6});
            this.dgvUserBuy.Location = new System.Drawing.Point(3, 109);
            this.dgvUserBuy.Name = "dgvUserBuy";
            this.dgvUserBuy.RowHeadersVisible = false;
            this.dgvUserBuy.Size = new System.Drawing.Size(261, 624);
            this.dgvUserBuy.TabIndex = 11;
            this.dgvUserBuy.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAccEngine_CellClick);
            // 
            // Column2
            // 
            this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column2.HeaderText = "Username";
            this.Column2.Name = "Column2";
            this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column2.Width = 61;
            // 
            // Password
            // 
            this.Password.HeaderText = "Password";
            this.Password.Name = "Password";
            // 
            // Login
            // 
            this.Login.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Login.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Login.HeaderText = "";
            this.Login.Name = "Login";
            this.Login.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Login.Text = "Login";
            this.Login.Width = 21;
            // 
            // Column6
            // 
            this.Column6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column6.HeaderText = "";
            this.Column6.Name = "Column6";
            this.Column6.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column6.Text = "$";
            this.Column6.UseColumnTextForLinkValue = true;
            this.Column6.Width = 21;
            // 
            // dgvUserSell
            // 
            this.dgvUserSell.AllowUserToDeleteRows = false;
            this.dgvUserSell.AllowUserToResizeRows = false;
            this.dgvUserSell.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUserSell.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUserSell.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewButtonColumn1,
            this.Column7});
            this.dgvUserSell.Location = new System.Drawing.Point(270, 109);
            this.dgvUserSell.Name = "dgvUserSell";
            this.dgvUserSell.RowHeadersVisible = false;
            this.dgvUserSell.Size = new System.Drawing.Size(262, 624);
            this.dgvUserSell.TabIndex = 11;
            this.dgvUserSell.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAccEngine_CellClick);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn1.HeaderText = "Username";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn1.Width = 61;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Password";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewButtonColumn1
            // 
            this.dataGridViewButtonColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewButtonColumn1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.dataGridViewButtonColumn1.HeaderText = "";
            this.dataGridViewButtonColumn1.Name = "dataGridViewButtonColumn1";
            this.dataGridViewButtonColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewButtonColumn1.Text = "Login";
            this.dataGridViewButtonColumn1.Width = 21;
            // 
            // Column7
            // 
            this.Column7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Column7.HeaderText = "";
            this.Column7.Name = "Column7";
            this.Column7.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column7.Text = "$";
            this.Column7.UseColumnTextForLinkValue = true;
            this.Column7.Width = 21;
            // 
            // cboMarket
            // 
            this.cboMarket.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMarket.FormattingEnabled = true;
            this.cboMarket.Items.AddRange(new object[] {
            "Live",
            "NonLive",
            "All"});
            this.cboMarket.Location = new System.Drawing.Point(465, 12);
            this.cboMarket.Name = "cboMarket";
            this.cboMarket.Size = new System.Drawing.Size(67, 21);
            this.cboMarket.TabIndex = 25;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(388, 39);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Giá tiền quét";
            // 
            // txtUserscan
            // 
            this.txtUserscan.Location = new System.Drawing.Point(300, 13);
            this.txtUserscan.Name = "txtUserscan";
            this.txtUserscan.Size = new System.Drawing.Size(92, 20);
            this.txtUserscan.TabIndex = 26;
            // 
            // txtPassScan
            // 
            this.txtPassScan.Location = new System.Drawing.Point(398, 13);
            this.txtPassScan.Name = "txtPassScan";
            this.txtPassScan.Size = new System.Drawing.Size(60, 20);
            this.txtPassScan.TabIndex = 26;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(246, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(51, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Tk quét";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(167, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(67, 47);
            this.button1.TabIndex = 27;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnSaveExchange
            // 
            this.btnSaveExchange.Location = new System.Drawing.Point(240, 66);
            this.btnSaveExchange.Name = "btnSaveExchange";
            this.btnSaveExchange.Size = new System.Drawing.Size(22, 26);
            this.btnSaveExchange.TabIndex = 28;
            this.btnSaveExchange.TabStop = false;
            this.btnSaveExchange.Click += new System.EventHandler(this.btnSaveExchange_Click);
            // 
            // btnSaveExchange2
            // 
            this.btnSaveExchange2.Location = new System.Drawing.Point(510, 63);
            this.btnSaveExchange2.Name = "btnSaveExchange2";
            this.btnSaveExchange2.Size = new System.Drawing.Size(22, 26);
            this.btnSaveExchange2.TabIndex = 28;
            this.btnSaveExchange2.TabStop = false;
            this.btnSaveExchange2.Click += new System.EventHandler(this.btnSaveExchange_Click);
            // 
            // btnLoginBuyGroup
            // 
            this.btnLoginBuyGroup.Location = new System.Drawing.Point(4, 79);
            this.btnLoginBuyGroup.Name = "btnLoginBuyGroup";
            this.btnLoginBuyGroup.Size = new System.Drawing.Size(26, 24);
            this.btnLoginBuyGroup.TabIndex = 29;
            this.btnLoginBuyGroup.TabStop = false;
            this.btnLoginBuyGroup.Click += new System.EventHandler(this.btnLoginGroup_Click);
            // 
            // btnLoginSellGroup
            // 
            this.btnLoginSellGroup.Location = new System.Drawing.Point(272, 76);
            this.btnLoginSellGroup.Name = "btnLoginSellGroup";
            this.btnLoginSellGroup.Size = new System.Drawing.Size(26, 27);
            this.btnLoginSellGroup.TabIndex = 29;
            this.btnLoginSellGroup.TabStop = false;
            this.btnLoginSellGroup.Click += new System.EventHandler(this.btnLoginGroup_Click);
            // 
            // ckBuyAccRandomIp
            // 
            this.ckBuyAccRandomIp.AutoSize = true;
            this.ckBuyAccRandomIp.Location = new System.Drawing.Point(36, 89);
            this.ckBuyAccRandomIp.Name = "ckBuyAccRandomIp";
            this.ckBuyAccRandomIp.Size = new System.Drawing.Size(78, 17);
            this.ckBuyAccRandomIp.TabIndex = 30;
            this.ckBuyAccRandomIp.Text = "Random Ip";
            this.ckBuyAccRandomIp.UseVisualStyleBackColor = true;
            // 
            // ckSellAccRandomIp
            // 
            this.ckSellAccRandomIp.AutoSize = true;
            this.ckSellAccRandomIp.Location = new System.Drawing.Point(308, 88);
            this.ckSellAccRandomIp.Name = "ckSellAccRandomIp";
            this.ckSellAccRandomIp.Size = new System.Drawing.Size(78, 17);
            this.ckSellAccRandomIp.TabIndex = 30;
            this.ckSellAccRandomIp.Text = "Random Ip";
            this.ckSellAccRandomIp.UseVisualStyleBackColor = true;
            // 
            // txtIpFakeScan
            // 
            this.txtIpFakeScan.Location = new System.Drawing.Point(270, 36);
            this.txtIpFakeScan.Name = "txtIpFakeScan";
            this.txtIpFakeScan.ReadOnly = true;
            this.txtIpFakeScan.Size = new System.Drawing.Size(112, 20);
            this.txtIpFakeScan.TabIndex = 31;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(247, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "IP:";
            // 
            // frmSboBet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1094, 733);
            this.Controls.Add(this.txtIpFakeScan);
            this.Controls.Add(this.ckSellAccRandomIp);
            this.Controls.Add(this.ckBuyAccRandomIp);
            this.Controls.Add(this.btnLoginSellGroup);
            this.Controls.Add(this.btnLoginBuyGroup);
            this.Controls.Add(this.btnSaveExchange2);
            this.Controls.Add(this.btnSaveExchange);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtPassScan);
            this.Controls.Add(this.txtUserscan);
            this.Controls.Add(this.cboMarket);
            this.Controls.Add(this.dgvUserSell);
            this.Controls.Add(this.dgvUserBuy);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtBuyEx);
            this.Controls.Add(this.txtSellEx);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.numMaxStake);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnStart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(900, 682);
            this.Name = "frmSboBet";
            this.Text = "BetRunDown  - © 2015";
            ((System.ComponentModel.ISupportInitialize)(this.numMaxStake)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSellAfterSecond)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLogScan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserBuy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserSell)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSaveExchange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSaveExchange2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnLoginBuyGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnLoginSellGroup)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.NumericUpDown numMaxStake;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dgvLogScan;
        private System.Windows.Forms.RadioButton rdBetAway;
        private System.Windows.Forms.RadioButton rdBetHome;
        private System.Windows.Forms.Button btnTrade;
        private System.Windows.Forms.TextBox txtSellEx;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBuyEx;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.DataGridView dgvUserBuy;
        private System.Windows.Forms.DataGridView dgvUserSell;
        private System.Windows.Forms.ComboBox cboMarket;
        private System.Windows.Forms.TextBox txtStake;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtUserscan;
        private System.Windows.Forms.TextBox txtPassScan;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Home;
        private System.Windows.Forms.DataGridViewTextBoxColumn Away;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Odd;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox btnSaveExchange;
        private System.Windows.Forms.PictureBox btnSaveExchange2;
        private System.Windows.Forms.PictureBox btnLoginBuyGroup;
        private System.Windows.Forms.PictureBox btnLoginSellGroup;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Password;
        private System.Windows.Forms.DataGridViewButtonColumn Login;
        private System.Windows.Forms.DataGridViewLinkColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewButtonColumn dataGridViewButtonColumn1;
        private System.Windows.Forms.DataGridViewLinkColumn Column7;
        private System.Windows.Forms.CheckBox ckBuyAccRandomIp;
        private System.Windows.Forms.CheckBox ckSellAccRandomIp;
        private System.Windows.Forms.TextBox txtIpFakeScan;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numSellAfterSecond;
        private System.Windows.Forms.CheckBox ckSellAfter;
        private System.Windows.Forms.Label label2;
    }
}

