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
    public partial class RemapForm : Form
    {
        FormMain MForm { get; set; }
        IAxxessBoard Device { get { return MForm.attachedDevice; } }

        public RemapForm(FormMain mForm)
        {
            MForm = mForm;

            InitializeComponent();
        }

        private void RemapForm_Load(object sender, EventArgs e)
        {
            if (this.MForm.CurrentOperation != null)
            {
                if (this.MForm.CurrentOperation.Type.Equals(OperationType.Boot))
                {
                    this.MForm.CurrentOperation.Stop();
                    this.MForm.CurrentOperation.WorkerThread.Join();
                    Thread.Sleep(1000);
                    this.Device.SendASWCRequestPacket();
                }
                else if (!this.MForm.CurrentOperation.Status.Equals(OperationStatus.Working))
                {
                    this.Device.SendASWCRequestPacket();
                }
                else 
                {
                    this.Close();
                    return;
                }
            }
        }
    }
}
