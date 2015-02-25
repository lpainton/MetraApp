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
    public partial class FormMain : Form
    {
        const int VID = 1240;
        const int PID = 63301;

        public const int DbtDeviceArrival = 0x8000;
        public const int DbtDeviceremovecomplete = 0x8004; // device is gone
        public const int WmDevicechange = 0x0219; //device change event

        public HIDChecksumBoard attachedDevice;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            attachedDevice = null;

            mainLabel.Text = "Board ID: - \nFirmware Version: -";
            mainStatusLabel.Text = "Initializing";
            UpdateBoardInfo();
        }

        private void UpdateBoardInfo()
        {
            attachedDevice = (HIDChecksumBoard)HIDDevice.FindDevice(VID, PID, typeof(HIDChecksumBoard));
            if (attachedDevice == null)
            {
                mainLabel.Text = "Board ID: -\nFirmware Ver: -";
                mainStatusLabel.Text = "No board connected";
            }
            else
            {
                attachedDevice.SpinupIntro();
                Thread.Sleep(200);
                mainLabel.Text = "Board ID: " + attachedDevice.ProductID + "\nFirmware Ver: " + attachedDevice.AppFirmwareVersion;
            }
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

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WmDevicechange)
            {
                UpdateBoardInfo();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
