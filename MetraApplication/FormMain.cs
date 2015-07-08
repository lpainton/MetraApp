using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using Metra.Axxess;

namespace MetraApplication
{
    public enum AppStatus
    {
        NoDevice,
        DeviceConnected,
        Streaming,
    };

    public partial class FormMain : Form
    {

        delegate void UpdateCallback();

        //This set of constants represent signals for system messages related to device events
        public const int DbtDeviceArrival = 0x8000; //A new device has been added
        public const int DbtDeviceremovecomplete = 0x8004; //A device has been completely removed
        public const int WmDevicechange = 0x0219; //Generic device change event

        public IAxxessBoard attachedDevice;
        public AppStatus Status { get; set; }
        public IOperation CurrentOperation { get; private set; }
        System.Windows.Forms.Timer CallbackTimer { get; set; }

        FileManager FManager { get; set; }
        ErrorManager EManager { get; set; }

        public FormMain()
        {
            EManager = new ErrorManager(this);
            FManager = new FileManager(EManager);
            this.FormClosing += new FormClosingEventHandler((s, a) =>
            {
                if (this.attachedDevice != null) this.attachedDevice.Dispose();
                if (this.CurrentOperation != null) this.CurrentOperation.Stop();
            });

            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            attachedDevice = null;
            CurrentOperation = null;

            CallbackTimer = new System.Windows.Forms.Timer();
            CallbackTimer.Interval = 500;
            CallbackTimer.Start();
            CallbackTimer.Tick += FormUpdate;

            mainStatusLabel.Text = "Initializing";
            mainProgressBar.Value = 0;

            //Configure labels
            connectLabel.Text = "Connect to a device via USB.\nClick this first then plug in your device.";
            remapLabel.Text = "Remap the buttons on the device.";
            inetLabel.Text = "Update device firmware using the\nlatest files from the internet.";
            fileLabel.Text = "Specify a firmware file to load\non the device.";
            mainProgressBar.Value = 10;

            //Set initial status
            this.Status = AppStatus.NoDevice;
            mainProgressBar.Value = 20;

            mainStatusLabel.Text = "File system check";
            mainProgressBar.Value = 30;
            FManager.CreateDirectory(FManager.FirmwareFolder);
            if (!File.Exists(FManager.FirmwareArchive))
            {
                SetupForm sf = new SetupForm(FManager);
                sf.ShowDialog();
            }

            FManager.CreateDirectory(FManager.MapFolder);

            this.UpdateControls();
            mainStatusLabel.Text = "System ready and waiting for device";
            mainProgressBar.Value = 100;
        }

        /// <summary>
        /// Update logic for the form.  Updates are on-demand, not automatic.
        /// </summary>
        private void UpdateControls()
        {
            switch (this.Status)
            {
                case AppStatus.NoDevice:
                    connectButton.Enabled = true;
                    remapButton.Enabled = false;
                    inetButton.Enabled = false;
                    fileButton.Enabled = false;
                    break;

                case AppStatus.DeviceConnected:
                    connectButton.Enabled = false;
                    remapButton.Enabled = true;
                    inetButton.Enabled = true;
                    fileButton.Enabled = true;
                    break;

                case AppStatus.Streaming:
                    connectButton.Enabled = false;
                    remapButton.Enabled = false;
                    inetButton.Enabled = false;
                    fileButton.Enabled = false;
                    break;
            }

            //If there's a device then we should be able to get info on it
            devInfoButton.Enabled = (attachedDevice != null);
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            this.UpdateFromInternet();
        }

        /// <summary>
        /// This function is called whenever the form is notified of a windows system message.
        /// </summary>
        /// <param name="m">The specific message sent to the form.</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            //Here we are only interested in device changes
            //if (m.Msg == WmDevicechange)
            //{
            //}
        }

        public void OnDeviceRemoved(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                // after we've done all the processing, 
                this.Invoke(new MethodInvoker(delegate
                {
                    this.attachedDevice.Dispose();
                    this.attachedDevice = null;
                    if (this.CurrentOperation != null)
                    {
                        CurrentOperation.Stop();
                        CurrentOperation = null;
                        this.mainProgressBar.Value = 0;
                    }

                    this.Status = AppStatus.NoDevice;
                    this.UpdateControls();

                    this.mainStatusLabel.Text = "Device was removed";
                }));
                return;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConnectionForm cf = new ConnectionForm();
            cf.ShowDialog();
            this.attachedDevice = cf.Device;
            if (this.attachedDevice != null)
            {
                this.CurrentOperation = OperationFactory.SpawnOperation(OperationType.Boot, new OpArgs(this.attachedDevice));
                this.CurrentOperation.Start();

                this.Status = AppStatus.DeviceConnected;
                this.UpdateControls();

                this.attachedDevice.AddRemovedEvent(OnDeviceRemoved);

                mainStatusLabel.Text = "Device was found and connected";
            }
        }

        private void fileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            
            //fileDialog.Filter = "Hex files (.hex) | *.hex";
            fileDialog.FilterIndex = 1;

            fileDialog.Multiselect = false;

            DialogResult result = fileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.UpdateFromFile(fileDialog.FileName);
            }
        }

        private void UpdateFromInternet()
        {
            this.Status = AppStatus.Streaming;
            if (!File.Exists(FManager.ManifestFile) && FManager.IsInternetAvailable())
            {
                FManager.DownloadManifest(FManager.ManifestURL);
            }
            
            while(FManager.Web.IsBusy)
            {
                Thread.Sleep(100);
            }

            string fileName = FManager.SearchManifest(this.attachedDevice.ProductID);

            if (fileName.Equals(String.Empty))
            {
                MessageBox.Show("Could not determine a firmware file for the connected device!");
                this.Status = AppStatus.DeviceConnected;
            }
            else
            {
                UpdateFromFile(FManager.GetPathToFirmware(fileName));
            }
        }

        private void UpdateFromFile(string path)
        {
            //Checkpoint
            if (this.CurrentOperation == null || !this.CurrentOperation.Type.Equals(OperationType.Boot))
                return;

            this.mainStatusLabel.Text = "Updating app firmware...";

            this.CurrentOperation.Stop();
            this.CurrentOperation.WorkerThread.Join();
            this.CurrentOperation = OperationFactory.SpawnOperation(OperationType.Firmware, new OpArgs(this.attachedDevice, path));
            this.CurrentOperation.Start();

            this.Status = AppStatus.Streaming;
            this.UpdateControls();

            //this.mainStatusLabel.Text = "Firmware update complete";
            //this.Status = AppStatus.DeviceConnected;
            //this.UpdateControls();
        }


        /// <summary>
        /// Callback updates progress bar when an operation is in effect.
        /// </summary>
        /// <param name="s">Sender</param>
        /// <param name="e">EventArgs object</param>
        private void FormUpdate(object s, EventArgs e)
        {
            UpdateControls();
            UpdateProgress();
            if (this.CurrentOperation != null)
                CatchErrors();
        }

        public void CatchErrors()
        {
            if (this.statusStrip1.InvokeRequired)
            {
                UpdateCallback p = new UpdateCallback(CatchErrors);
                this.Invoke(p);
            }
            else
            {
                if (this.CurrentOperation.Error != null)
                {
                    if (this.CurrentOperation.Error is TimeoutException)
                    {
                        this.OnDeviceRemoved(CurrentOperation, new EventArgs());
                    }
                    else
                    {
                        string message = this.CurrentOperation.Error.Message;
                        this.CurrentOperation = null;
                        this.Status = AppStatus.NoDevice;
                        this.attachedDevice.Dispose();
                        this.attachedDevice = null;
                        MessageBox.Show(message + "\nPlease reconnect your device.");
                    }
                }
            }
        }

        public void UpdateProgress()
        {
            if (this.statusStrip1.InvokeRequired)
            {
                UpdateCallback p = new UpdateCallback(UpdateProgress);
                this.Invoke(p);
            }
            else
            {
                if (this.CurrentOperation != null && !this.CurrentOperation.Equals(OperationType.Boot))
                {
                    if (this.CurrentOperation.Status.Equals(OperationStatus.Working))
                        this.mainStatusLabel.Text = this.CurrentOperation.Type.ToString() + " operation in progress...";
                    else if (this.CurrentOperation.Status.Equals(OperationStatus.Finished))
                        this.mainStatusLabel.Text = this.CurrentOperation.Type.ToString() + " operation completed.";
                    this.mainProgressBar.Value = this.CurrentOperation.Progress;
                }
            }
        }

        private void remapButton_Click(object sender, EventArgs e)
        {
            RemapForm rForm = new RemapForm(this);
            rForm.ShowDialog();
        }

        private void devInfoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(attachedDevice.Info);
        }
    }
}
