using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;

namespace MetraWPFBrowserApp
{
    class AboutButtonLogic
    {
        Button AboutButton { get; set; }
        delegate void EnableDelegate();

        public AboutButtonLogic(Button connectButton)
        {
            this.AboutButton = connectButton;
        }

        public void Update(AppState state)
        {
            if (state.Equals(AppState.DeviceConnected))
            {
                this.Disable();
            }
            else if (!state.Equals(AppState.DeviceConnected))
            {
                this.Enable();
            }
        }

        public void Disable() 
        {
            if (this.AboutButton.Dispatcher.CheckAccess())
            {
                this.AboutButton.IsEnabled = false;
            }
            else
            {
                this.AboutButton.Dispatcher.BeginInvoke(
                    new EnableDelegate(Disable),
                    DispatcherPriority.Normal,
                    new object[] { });
            }
        }
        public void Enable() 
        {
            if (this.AboutButton.Dispatcher.CheckAccess())
            {
                this.AboutButton.IsEnabled = true;
            }
            else
            {
                this.AboutButton.Dispatcher.BeginInvoke(
                    new EnableDelegate(Enable),
                    DispatcherPriority.Normal,
                    new object[] { });
            }
        }

        public void OnClick(object sender, EventArgs e)
        {
            System.Reflection.Assembly asm = typeof(AboutButtonLogic).Assembly;
            System.Reflection.AssemblyName name = asm.GetName();
            System.Windows.MessageBox.Show("Application Version: " + name.Version.ToString() + " BETA");
        }
    }
}
