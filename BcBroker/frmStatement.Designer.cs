namespace BcBroker
{
    partial class frmStatement
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmStatement));
            this.webStatement = new System.Windows.Forms.WebBrowser();
            this.dateHistoryPick = new System.Windows.Forms.DateTimePicker();
            this.btnHistory = new System.Windows.Forms.Button();
            this.btnBetList = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // webStatement
            // 
            this.webStatement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webStatement.Location = new System.Drawing.Point(0, 43);
            this.webStatement.MinimumSize = new System.Drawing.Size(20, 20);
            this.webStatement.Name = "webStatement";
            this.webStatement.ScriptErrorsSuppressed = true;
            this.webStatement.Size = new System.Drawing.Size(969, 510);
            this.webStatement.TabIndex = 2;
            // 
            // dateHistoryPick
            // 
            this.dateHistoryPick.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateHistoryPick.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateHistoryPick.Location = new System.Drawing.Point(195, 3);
            this.dateHistoryPick.Name = "dateHistoryPick";
            this.dateHistoryPick.Size = new System.Drawing.Size(105, 23);
            this.dateHistoryPick.TabIndex = 8;
            // 
            // btnHistory
            // 
            this.btnHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHistory.Location = new System.Drawing.Point(103, 3);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Size = new System.Drawing.Size(75, 23);
            this.btnHistory.TabIndex = 7;
            this.btnHistory.Text = "History";
            this.btnHistory.UseVisualStyleBackColor = true;
            this.btnHistory.Click += new System.EventHandler(this.btnHistory_Click);
            // 
            // btnBetList
            // 
            this.btnBetList.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBetList.Location = new System.Drawing.Point(12, 3);
            this.btnBetList.Name = "btnBetList";
            this.btnBetList.Size = new System.Drawing.Size(75, 23);
            this.btnBetList.TabIndex = 6;
            this.btnBetList.Text = "Bet List";
            this.btnBetList.UseVisualStyleBackColor = true;
            this.btnBetList.Click += new System.EventHandler(this.btnBetList_Click);
            // 
            // frmStatement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(969, 553);
            this.Controls.Add(this.dateHistoryPick);
            this.Controls.Add(this.btnHistory);
            this.Controls.Add(this.btnBetList);
            this.Controls.Add(this.webStatement);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmStatement";
            this.Text = "Statement";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webStatement;
        private System.Windows.Forms.DateTimePicker dateHistoryPick;
        private System.Windows.Forms.Button btnHistory;
        private System.Windows.Forms.Button btnBetList;
    }
}