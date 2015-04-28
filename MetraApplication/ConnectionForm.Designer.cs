namespace MetraApplication
{
    partial class ConnectionForm
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
            this.mainLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mainLabel
            // 
            this.mainLabel.AutoSize = true;
            this.mainLabel.Location = new System.Drawing.Point(42, 33);
            this.mainLabel.Name = "mainLabel";
            this.mainLabel.Size = new System.Drawing.Size(114, 13);
            this.mainLabel.TabIndex = 0;
            this.mainLabel.Text = "Searching for device...";
            this.mainLabel.Click += new System.EventHandler(this.mainLabel_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(58, 73);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // ConnectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(191, 125);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.mainLabel);
            this.Name = "ConnectionForm";
            this.Text = "Connect To Device";
            this.Load += new System.EventHandler(this.ConnectionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label mainLabel;
        private System.Windows.Forms.Button cancelButton;
    }
}