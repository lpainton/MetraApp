namespace MetraApplication
{
    partial class FormMain
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.mainProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.mainStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.connectLabel = new System.Windows.Forms.Label();
            this.inetButton = new System.Windows.Forms.Button();
            this.connectButton = new System.Windows.Forms.Button();
            this.remapButton = new System.Windows.Forms.Button();
            this.fileButton = new System.Windows.Forms.Button();
            this.remapLabel = new System.Windows.Forms.Label();
            this.inetLabel = new System.Windows.Forms.Label();
            this.fileLabel = new System.Windows.Forms.Label();
            this.devInfoButton = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mainProgressBar,
            this.mainStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 146);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(467, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.statusStrip1_ItemClicked);
            // 
            // mainProgressBar
            // 
            this.mainProgressBar.Name = "mainProgressBar";
            this.mainProgressBar.Size = new System.Drawing.Size(200, 16);
            this.mainProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // mainStatusLabel
            // 
            this.mainStatusLabel.Name = "mainStatusLabel";
            this.mainStatusLabel.Size = new System.Drawing.Size(118, 17);
            this.mainStatusLabel.Text = "toolStripStatusLabel1";
            // 
            // connectLabel
            // 
            this.connectLabel.AutoSize = true;
            this.connectLabel.Location = new System.Drawing.Point(219, 0);
            this.connectLabel.Name = "connectLabel";
            this.connectLabel.Size = new System.Drawing.Size(72, 13);
            this.connectLabel.TabIndex = 3;
            this.connectLabel.Text = "connectLabel";
            // 
            // inetButton
            // 
            this.inetButton.Location = new System.Drawing.Point(0, 70);
            this.inetButton.Name = "inetButton";
            this.inetButton.Size = new System.Drawing.Size(213, 31);
            this.inetButton.TabIndex = 4;
            this.inetButton.Text = "Update Firmware from the Internet";
            this.inetButton.UseVisualStyleBackColor = true;
            this.inetButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(0, 0);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(213, 29);
            this.connectButton.TabIndex = 5;
            this.connectButton.Text = "Connect to Device";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // remapButton
            // 
            this.remapButton.Location = new System.Drawing.Point(0, 34);
            this.remapButton.Name = "remapButton";
            this.remapButton.Size = new System.Drawing.Size(213, 30);
            this.remapButton.TabIndex = 6;
            this.remapButton.Text = "Remap Buttons";
            this.remapButton.UseVisualStyleBackColor = true;
            this.remapButton.Click += new System.EventHandler(this.remapButton_Click);
            // 
            // fileButton
            // 
            this.fileButton.Location = new System.Drawing.Point(0, 107);
            this.fileButton.Name = "fileButton";
            this.fileButton.Size = new System.Drawing.Size(213, 32);
            this.fileButton.TabIndex = 7;
            this.fileButton.Text = "Update Firmware from File";
            this.fileButton.UseVisualStyleBackColor = true;
            this.fileButton.Click += new System.EventHandler(this.fileButton_Click);
            // 
            // remapLabel
            // 
            this.remapLabel.AutoSize = true;
            this.remapLabel.Location = new System.Drawing.Point(219, 34);
            this.remapLabel.Name = "remapLabel";
            this.remapLabel.Size = new System.Drawing.Size(62, 13);
            this.remapLabel.TabIndex = 8;
            this.remapLabel.Text = "remapLabel";
            // 
            // inetLabel
            // 
            this.inetLabel.AutoSize = true;
            this.inetLabel.Location = new System.Drawing.Point(219, 70);
            this.inetLabel.Name = "inetLabel";
            this.inetLabel.Size = new System.Drawing.Size(50, 13);
            this.inetLabel.TabIndex = 9;
            this.inetLabel.Text = "inetLabel";
            // 
            // fileLabel
            // 
            this.fileLabel.AutoSize = true;
            this.fileLabel.Location = new System.Drawing.Point(219, 107);
            this.fileLabel.Name = "fileLabel";
            this.fileLabel.Size = new System.Drawing.Size(46, 13);
            this.fileLabel.TabIndex = 10;
            this.fileLabel.Text = "fileLabel";
            // 
            // devInfoButton
            // 
            this.devInfoButton.Location = new System.Drawing.Point(441, 120);
            this.devInfoButton.Name = "devInfoButton";
            this.devInfoButton.Size = new System.Drawing.Size(26, 23);
            this.devInfoButton.TabIndex = 11;
            this.devInfoButton.Text = "?";
            this.devInfoButton.UseVisualStyleBackColor = true;
            this.devInfoButton.Click += new System.EventHandler(this.devInfoButton_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 168);
            this.Controls.Add(this.devInfoButton);
            this.Controls.Add(this.fileLabel);
            this.Controls.Add(this.inetLabel);
            this.Controls.Add(this.remapLabel);
            this.Controls.Add(this.fileButton);
            this.Controls.Add(this.remapButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.inetButton);
            this.Controls.Add(this.connectLabel);
            this.Controls.Add(this.statusStrip1);
            this.Name = "FormMain";
            this.Text = "Metra App for Windows";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar mainProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel mainStatusLabel;
        private System.Windows.Forms.Label connectLabel;
        private System.Windows.Forms.Button inetButton;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button remapButton;
        private System.Windows.Forms.Button fileButton;
        private System.Windows.Forms.Label remapLabel;
        private System.Windows.Forms.Label inetLabel;
        private System.Windows.Forms.Label fileLabel;
        private System.Windows.Forms.Button devInfoButton;
    }
}

