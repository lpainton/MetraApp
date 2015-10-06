using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;

namespace MetraWPFBrowserApp
{
    class UpdateButtonLogic
    {
        Button UpdateButton { get; set; }
        delegate void EnableDelegate();

        public UpdateButtonLogic(Button connectButton)
        {
            this.UpdateButton = connectButton;
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
            if (this.UpdateButton.Dispatcher.CheckAccess())
            {
                this.UpdateButton.IsEnabled = false;
            }
            else
            {
                this.UpdateButton.Dispatcher.BeginInvoke(
                    new EnableDelegate(Disable),
                    DispatcherPriority.Normal,
                    new object[] { });
            }
        }
        public void Enable() 
        {
            if (this.UpdateButton.Dispatcher.CheckAccess())
            {
                this.UpdateButton.IsEnabled = true;
            }
            else
            {
                this.UpdateButton.Dispatcher.BeginInvoke(
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
