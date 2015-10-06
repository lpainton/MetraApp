using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Timers;
using System.IO;
using System.Windows.Threading;

namespace MetraWPFBrowserApp
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup : Window
    {
        delegate void ProgressDelegate();

        Timer time;
        FileManager FManager { get; set; }
        bool Done { get; set; }

        public Setup(FileManager fman)
        {

            this.FManager = fman;
            time = new Timer();

            this.Done = false;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TickCallback(object s, System.Timers.ElapsedEventArgs e)
        {
            UpdateProgress();

            if (File.Exists(FManager.FirmwareArchive) && !FManager.Web.IsBusy && !Done)
            {
                FManager.UnpackFirmwareArchive();
                this.Done = true;
            }
        }

        public void UpdateProgress()
        {
            if (!this.progressBar1.Dispatcher.CheckAccess())
            {
                this.progressBar1.Dispatcher.BeginInvoke(
                    new ProgressDelegate(UpdateProgress),
                    DispatcherPriority.Normal,
                    new object[] { });
            }
            else
            {
                if (this.FManager.Web.IsBusy)
                {
                    this.label1.Content = "Download in progress...";
                    this.progressBar1.Value = (this.progressBar1.Value < 100) ? progressBar1.Value + 1 : 0;
                }
                else
                {
                    if (this.Done)
                    {
                        this.label1.Content = "Setup complete!";
                        this.progressBar1.Value = 100;
                        this.okButton.IsEnabled = true;
                    }
                }
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            time.Stop();
            time.Elapsed -= (TickCallback);
            time.Dispose();
        }

        private void SetupForm_Load(object sender, RoutedEventArgs e)
        {
            this.okButton.IsEnabled = false;
            time.Elapsed += (TickCallback);
            time.Interval = 500;
            time.Start();

            if (FManager.IsInternetAvailable())
                this.FManager.DownloadArchive(FManager.BatchURL);
            else
            {
                this.label1.Content = "No internet...";
                this.Done = true;
            }
        }
    }
}
