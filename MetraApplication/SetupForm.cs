using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Metra.Axxess;

namespace MetraApplication
{
    /// <summary>
    /// Currenly not being used.
    /// </summary>
    public partial class SetupForm : Form
    {
        delegate void ProgressCallback();

        Timer time;
        FileManager FManager { get; set; }
        bool Done { get; set; }

        public SetupForm(FileManager fman)
        {
            InitializeComponent();

            this.okButton.Enabled = false;
            this.FManager = fman;
            time = new Timer();

            this.Done = false;

            if (FManager.IsInternetAvailable())
                this.FManager.DownloadArchive(FManager.BatchURL);
            else
            {
                this.label1.Text = "No internet...";
                this.Done = true;
            }
        }

 

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UpdateForm_Load(object sender, EventArgs e)
        {
            this.okButton.Enabled = false;
            time.Tick += (TickCallback);
            time.Interval = 500;
            time.Start();
        }

        private void TickCallback(object s, EventArgs e)
        {
            UpdateProgress();

            if (File.Exists(FManager.FirmwareArchive) && !FManager.Web.IsBusy && !Done)
            {
                FManager.UnpackFirmwareArchive();
                this.Done = true;
            }

            this.okButton.Enabled = this.Done;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            time.Stop();
            time.Tick -= (TickCallback);
        }

        public void UpdateProgress()
        {
            if (this.progressBar1.InvokeRequired)
            {
                ProgressCallback p = new ProgressCallback(UpdateProgress);
                this.Invoke(p);
            }
            else
            {
                if (this.FManager.Web.IsBusy)
                {
                    this.label1.Text = "Download in progress...";
                    this.progressBar1.Value = (this.progressBar1.Value < 100) ? progressBar1.Value + 1 : 0;
                }
                else
                {
                    if (this.Done)
                    {
                        this.label1.Text = "Setup complete!";
                        this.progressBar1.Value = 100;
                    }
                }
            }
        }
    }
}
