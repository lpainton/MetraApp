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
        //This set of constants represent signals for system messages related to device events
        public const int DbtDeviceArrival = 0x8000; //A new device has been added
        public const int DbtDeviceremovecomplete = 0x8004; //A device has been completely removed
        public const int WmDevicechange = 0x0219; //Generic device change event

        public AxxessHIDBoard attachedDevice;
        public AppStatus Status { get; set; }

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            attachedDevice = null;

            mainStatusLabel.Text = "Initializing";
            mainProgressBar.Value = 0;

            //Configure labels
            connectLabel.Text = "Connect to a device via USB.\nPlease make sure device is physically\nconnected first.";
            remapLabel.Text = "Remap the buttons on the device.";
            inetLabel.Text = "Update device firmware using the\nlatest files from the internet.";
            fileLabel.Text = "Specify a firmware file to load\non the device.";
            mainProgressBar.Value = 10;

            //Set initial status
            this.Status = AppStatus.NoDevice;
            mainProgressBar.Value = 20;

            this.Update();
            mainStatusLabel.Text = "System ready and waiting for device";
            mainProgressBar.Value = 100;
        }

        /// <summary>
        /// Update logic for the form.  Updates are on-demand, not automatic.
        /// </summary>
        private void Update()
        {
            switch (this.Status)
            {
                case AppStatus.NoDevice:
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

                    connectButton.Enabled = true;
                    remapButton.Enabled = false;
                    inetButton.Enabled = false;
                    fileButton.Enabled = false;

                    this.mainStatusLabel.Text = "Device was removed";
                }));
                return;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.attachedDevice = AxxessHIDBoard.ConnectToBoard();
            if (this.attachedDevice != null)
            {
                connectButton.Enabled = false;
                //remapButton.Enabled = true;
                //inetButton.Enabled = true;
                fileButton.Enabled = true;

                this.attachedDevice.OnDeviceRemoved += OnDeviceRemoved;

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
            this.mainStatusLabel.Text = "Updating app firmware...";
            this.attachedDevice.UpdateAppFirmware(path, mainProgressBar);
            this.mainStatusLabel.Text = "Firmware update complete";
        }
    }
}
