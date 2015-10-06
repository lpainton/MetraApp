using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MetraWPFBrowserApp
{
    class AppUpdateTimer
    {
        System.Timers.Timer UpdateTimer { get; set; }

        public AppUpdateTimer(System.Timers.ElapsedEventHandler handler)
        {
            this.UpdateTimer = new System.Timers.Timer();

            this.UpdateTimer.Elapsed += handler;
        }

        public void Initialize()
        {
            this.UpdateTimer.Interval = 500;
            this.UpdateTimer.AutoReset = true;
        }

        public void Enable()
        {
            this.UpdateTimer.Enabled = true;
        }

        public void Disable()
        {
            this.UpdateTimer.Enabled = false;
        }

        public void Dispose()
        {
            this.UpdateTimer.Dispose();
        }
    }
}
