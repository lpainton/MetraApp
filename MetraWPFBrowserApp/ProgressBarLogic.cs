using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;
using Metra.Axxess;

namespace MetraWPFBrowserApp
{
    class ProgressBarLogic
    {
        ProgressBar ProgBar { get; set; }
        delegate void ProgressDelegate(int val);

        public ProgressBarLogic(ProgressBar bar)
        {
            this.ProgBar = bar;
        }

        public void Update(IOperation op)
        {
            if (op == null)
            {
                SetVal(0);
                return;
            }

            switch (op.Type)
            {
                case OperationType.Firmware:
                    SetVal(op.Progress);
                    break;

                default:
                    break;
            }
        }

        public void SetVal(int val)
        {
            if (this.ProgBar.Dispatcher.CheckAccess())
            {
                this.ProgBar.Value = val;
            }
            else
            {
                this.ProgBar.Dispatcher.BeginInvoke(
                    new ProgressDelegate(SetVal),
                    DispatcherPriority.Normal,
                    new object[] { val });
            }
        }
    }
}
