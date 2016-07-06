using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;

namespace MetraWPFBrowserApp
{
    class LoadButtonLogic
    {
        Button LoadButton { get; set; }
        delegate void EnableDelegate();

        public LoadButtonLogic(Button connectButton)
        {
            this.LoadButton = connectButton;
        }

        public void Update(AppState state)
        {
            if (state.Equals(AppState.DeviceConnected))
            {
                this.Enable();
            }
            else if (!state.Equals(AppState.DeviceConnected))
            {
                this.Disable();
            }
        }

        public void Disable() 
        {
            if (this.LoadButton.Dispatcher.CheckAccess())
            {
                this.LoadButton.IsEnabled = false;
            }
            else
            {
                this.LoadButton.Dispatcher.BeginInvoke(
                    new EnableDelegate(Disable),
                    DispatcherPriority.Normal,
                    new object[] { });
            }
        }
        public void Enable() 
        {
            if (this.LoadButton.Dispatcher.CheckAccess())
            {
                this.LoadButton.IsEnabled = true;
            }
            else
            {
                this.LoadButton.Dispatcher.BeginInvoke(
                    new EnableDelegate(Enable),
                    DispatcherPriority.Normal,
                    new object[] { });
            }
        }

        public void OnClick(object sender, EventArgs e)
        {

        }
    }
}
