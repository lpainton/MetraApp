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

namespace MetraApplication
{
    /// <summary>
    /// Currenly not being used.
    /// </summary>
    public partial class UpdateForm : Form
    {
        delegate void ProgressCallback();

        IOperation Op { get; set; }
        Timer time;

        public UpdateForm(IOperation op)
        {
            InitializeComponent();

            this.Op = op;
            time = new Timer();
        }

 

        private void okButton_Click(object sender, EventArgs e)
        {

        }

        private void UpdateForm_Load(object sender, EventArgs e)
        {
            this.okButton.Enabled = false;
            time.Tick += (TickCallback);
            time.Interval = 500;//how long you want it to stay.
            time.Start();
        }

        private void TickCallback(object s, EventArgs e)
        {
            UpdateProgress();
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
                this.progressBar1.Value = this.Op.Progress;
            }
        }
    }
}
