using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metra.Axxess;
using System.Threading;

namespace MetraWPFBrowserApp
{
    public partial class ConnectionForm : Form
    {
        delegate void OKCallback();
        delegate void LabelCallback(String text);
        public IAxxessBoard Device { get; private set; }
        Thread _workerThread;

        public ConnectionForm()
        {
            InitializeComponent();
        }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {
            //this.okButton.Enabled = false;
            this.cancelButton.Enabled = true;
            this.mainLabel.Text = "Searching for device...";

            ThreadStart workStart = new ThreadStart(ConnectionLoop);
            _workerThread = new Thread(workStart);
            _workerThread.Start();
        }


        private void UpdateLabel(String text)
        {
            if (this.mainLabel.InvokeRequired)
            {
                LabelCallback l = new LabelCallback(UpdateLabel);
                this.Invoke(l, new object[] { text });
            }
            else
            {
                this.mainLabel.Text = text;
            }
        }

        /*private void EnableOK()
        {
            if (this.okButton.InvokeRequired)
            {
                OKCallback d = new OKCallback(EnableOK);
                this.Invoke(d);
            }
            else
            {
                this.okButton.Enabled = true;
            }
        }*/

        private void CloseWindow() 
        {
            if (this.InvokeRequired)
            {
                OKCallback d = new OKCallback(CloseWindow);
                this.Invoke(d);
            }
            else
            {
                this.Close();
            }
        }

        private void ConnectionLoop()
        {
            while (this.Device == null)
            {
                this.Device = AxxessConnector.ResolveConnection();
                Thread.Sleep(10);
            }

            this.UpdateLabel("Device found.");
            //this.EnableOK();
            this.CloseWindow();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Device = null;
            this._workerThread.Abort();
        }

        /*private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }*/

        private void mainLabel_Click(object sender, EventArgs e)
        {
            return;
        }
    }
}
