namespace BcWin.UserControls
{
    partial class BetTabControl
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
            this.tabIbetSbobet = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // tabIbetSbobet
            // 
            this.tabIbetSbobet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabIbetSbobet.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabIbetSbobet.Location = new System.Drawing.Point(0, 0);
            this.tabIbetSbobet.Multiline = true;
            this.tabIbetSbobet.Name = "tabIbetSbobet";
            this.tabIbetSbobet.SelectedIndex = 0;
            this.tabIbetSbobet.Size = new System.Drawing.Size(590, 490);
            this.tabIbetSbobet.TabIndex = 0;
            // 
            // BetTabControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.Controls.Add(this.tabIbetSbobet);
            this.Name = "BetTabControl";
            this.Size = new System.Drawing.Size(590, 490);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabIbetSbobet;
    }
}
