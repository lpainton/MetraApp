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
        //const string FIRMWARE_FOLDER = "firmware";
        const string MAPS_FOLDER = "maps";
        const string CONFIG_FILE = "appconfig.cfg";
        const string FIRMWARE_ARCHIVE = "firmware.zip";
        const string FIRMWARE_MANIFEST = "firmware.txt";
        const string MANIFEST_URL = "http://axxessupdater.com/admin/secure/manifest-request.php";
        const string BATCH_URL = "http://axxessupdater.com/admin/secure/batch-download.php";

        delegate void ProgressCallback();

        //This set of constants represent signals for system messages related to device events
        public const int DbtDeviceArrival = 0x8000; //A new device has been added
        public const int DbtDeviceremovecomplete = 0x8004; //A device has been completely removed
        public const int WmDevicechange = 0x0219; //Generic device change event

        public IAxxessBoard attachedDevice;
        public AppStatus Status { get; set; }
        public IOperation CurrentOperation { get; private set; }
        System.Windows.Forms.Timer CallbackTimer { get; set; }

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            attachedDevice = null;
            CurrentOperation = null;

            CallbackTimer = new System.Windows.Forms.Timer();
            CallbackTimer.Interval = 500;
            CallbackTimer.Start();
            CallbackTimer.Tick += ProgressUpdate;

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
            FileSysCheck();

            this.UpdateControls();
            mainStatusLabel.Text = "System ready and waiting for device";
            mainProgressBar.Value = 100;
        }

        /// <summary>
        /// Checks if the needed directories/files for the application exist.  If not creates them.
        /// </summary>
        private void FileSysCheck()
        {
            /*if (!Directory.Exists(".\\" + FIRMWARE_FOLDER))
                Directory.CreateDirectory(".\\" + FIRMWARE_FOLDER);*/
            if (!Directory.Exists(".\\" + MAPS_FOLDER))
                Directory.CreateDirectory(".\\" + MAPS_FOLDER);
            //if (!File.Exists(CONFIG_FILE))
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
        }

        private void UpdateBoardInfo()
        {
            /*attachedDevice = (HIDChecksumBoard)HIDDevice.FindDevice(VID, PID, typeof(HIDChecksumBoard));
            if (attachedDevice == null)
            {
                connectLabel.Text = "Board ID: -\nFirmware Ver: -";
                mainStatusLabel.Text = "No board connected";
            }
            else
            {
                attachedDevice.SpinupIntro();
                Thread.Sleep(200);
                connectLabel.Text = "Board ID: " + attachedDevice.ProductID + "\nFirmware Ver: " + attachedDevice.AppFirmwareVersion;
            }*/
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void updateButton_Click(object sender, EventArgs e)
        {   
            if(!IsInternetAvailable())
            {
                mainStatusLabel.Text = "Could not connect to the internet!";
                return;
            }

            if (!File.Exists(FIRMWARE_ARCHIVE))
                DownloadFirmware();
            if (!File.Exists(FIRMWARE_MANIFEST))
                DownloadManifest();

            if (!CompareManifests())
                DownloadFirmware();

            string fileName = SearchManifest("CWI" + this.attachedDevice.ProductID.ToString());
            
            if (!fileName.Equals(String.Empty))
            {
                using (ZipArchive zip = ZipFile.OpenRead(FIRMWARE_ARCHIVE))
                {
                    foreach (ZipArchiveEntry entry in zip.Entries)
                        if (entry.Name == fileName)
                        {
                            if (File.Exists("firm.hex"))
                                File.Delete("firm.hex");
                            entry.ExtractToFile("firm.hex");
                        }
                }

                UpdateFromFile("firm.hex");

            }
        }

        private bool IsInternetAvailable()
        {
            mainStatusLabel.Text = "Checking for internet connectivity...";

            Ping myPing = new Ping();
            String host = "8.8.8.8";
            byte[] buffer = new byte[32];
            int timeout = 1000;
            PingOptions pingOptions = new PingOptions();
            PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
            return (reply.Status == IPStatus.Success);
        }

        private bool CompareManifests()
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(MANIFEST_URL, "temp");
            }

            string newManifest = File.ReadAllText("temp");
            string oldManifest = File.ReadAllText(FIRMWARE_MANIFEST);

            return (newManifest.Equals(oldManifest));
        }

        private string SearchManifest(string boardID)
        {
            foreach (string line in File.ReadAllLines(FIRMWARE_MANIFEST))
            {
                string[] entry = line.Split(',');
                if (entry[0].Equals(boardID))
                {
                    return entry[2];
                }
            }
            return String.Empty;
        }

        private void DownloadFirmware()
        {
            mainStatusLabel.Text = "Downloading latest firmware archive...";
            if (File.Exists(FIRMWARE_ARCHIVE))
                File.Delete(FIRMWARE_ARCHIVE);
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(BATCH_URL, FIRMWARE_ARCHIVE);
            }
            mainStatusLabel.Text = "Finshed downloading.";
        }

        private void DownloadManifest()
        {
            mainStatusLabel.Text = "Downloading latest firmware manifest...";
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(MANIFEST_URL, FIRMWARE_MANIFEST);
            }
            mainStatusLabel.Text = "Finshed downloading.";
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
                    this.attachedDevice = null;

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
                this.CurrentOperation = OperationFactory.SpawnOperation(OperationType.Idle, new OpArgs(this.attachedDevice));
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
            
            fileDialog.Filter = "Hex files (.hex) | *.hex";
            fileDialog.FilterIndex = 1;

            fileDialog.Multiselect = false;

            DialogResult result = fileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.UpdateFromFile(fileDialog.FileName);
            }
        }

        private void UpdateFromFile(string path)
        {
            if (this.CurrentOperation == null || this.CurrentOperation.Type != OperationType.Idle)
                return;

            this.mainStatusLabel.Text = "Updating app firmware...";

            this.CurrentOperation.Stop();
            this.CurrentOperation.WorkerThread.Join();
            this.CurrentOperation = OperationFactory.SpawnOperation(OperationType.Firmware, new OpArgs(this.attachedDevice, path));
            this.CurrentOperation.Start();

            this.Status = AppStatus.Streaming;
            this.UpdateControls();

            //UpdateForm uf = new UpdateForm(this.CurrentOperation);
            //uf.ShowDialog();

            /*while(this.CurrentOperation.Status.Equals(OperationStatus.Working))
            {
                this.mainProgressBar.Value = this.CurrentOperation.Progress;
                Thread.Sleep(200);
            }*/

            this.mainStatusLabel.Text = "Firmware update complete";

            this.Status = AppStatus.DeviceConnected;
            this.UpdateControls();
        }


        /// <summary>
        /// Callback updates progress bar when an operation is in effect.
        /// </summary>
        /// <param name="s">Sender</param>
        /// <param name="e">EventArgs object</param>
        private void ProgressUpdate(object s, EventArgs e)
        {
            UpdateProgress();
        }

        public void UpdateProgress()
        {
            if (this.statusStrip1.InvokeRequired)
            {
                ProgressCallback p = new ProgressCallback(UpdateProgress);
                this.Invoke(p);
            }
            else
            {
                if (this.CurrentOperation != null && !this.CurrentOperation.Equals(OperationType.Idle))
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
    }
}
