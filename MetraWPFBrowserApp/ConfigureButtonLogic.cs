using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;

namespace MetraWPFBrowserApp
{
    class ConfigureButtonLogic
    {
        Button ConfigureButton { get; set; }
        delegate void EnableDelegate();

        public ConfigureButtonLogic(Button connectButton)
        {
            this.ConfigureButton = connectButton;
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
            if (this.ConfigureButton.Dispatcher.CheckAccess())
            {
                this.ConfigureButton.IsEnabled = false;
            }
            else
            {
                this.ConfigureButton.Dispatcher.BeginInvoke(
                    new EnableDelegate(Disable),
                    DispatcherPriority.Normal,
                    new object[] { });
            }
        }
        public void Enable() 
        {
            if (this.ConfigureButton.Dispatcher.CheckAccess())
            {
                this.ConfigureButton.IsEnabled = true;
            }
            else
            {
                this.ConfigureButton.Dispatcher.BeginInvoke(
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
