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
    class ConnectButtonLogic
    {
        Button ConnectButton { get; set; }
        delegate void EnableDelegate();
        delegate void TextDelegate(string newText);

        public ConnectButtonLogic(Button connectButton)
        {
            this.ConnectButton = connectButton;
        }

        public void Update(AppState state)
        {
            switch (state)
            {
                case AppState.Loading:
                    Disable();
                    break;

                case AppState.Setup:
                    Disable();
                    break;

                case AppState.NoDevice:
                    Enable();
                    break;

                case AppState.DeviceConnected:
                    Disable();
                    break;
            }
        }

        public void Disable() 
        {
            if (this.ConnectButton.Dispatcher.CheckAccess())
            {
                this.ConnectButton.IsEnabled = false;
            }
            else
            {
                this.ConnectButton.Dispatcher.BeginInvoke(
                    new EnableDelegate(Disable),
                    DispatcherPriority.Normal,
                    new object[] { });
            }
        }
        public void Enable() 
        {
            if (this.ConnectButton.Dispatcher.CheckAccess())
            {
                this.ConnectButton.IsEnabled = true;
            }
            else
            {
                this.ConnectButton.Dispatcher.BeginInvoke(
                    new EnableDelegate(Enable),
                    DispatcherPriority.Normal,
                    new object[] { });
            }
        }

        public void ChangeText(string text)
        {
            if (!this.ConnectButton.Content.Equals(text))
            {
                if (this.ConnectButton.Dispatcher.CheckAccess())
                {
                    this.ConnectButton.Content = text;
                }
                else
                {
                    this.ConnectButton.Dispatcher.BeginInvoke(
                        new TextDelegate(ChangeText),
                        DispatcherPriority.Normal,
                        new object[] { text });
                }
            }
        }

        public void OnClick(out IAxxessBoard board)
        {
            ConnectionForm c = new ConnectionForm();
            c.ShowDialog();
            board = c.Device;
            c.Dispose();
        }
    }
}
