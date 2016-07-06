using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;

namespace MetraWPFBrowserApp
{
    class MainLabelLogic
    {
        public Label MainLbl { get; set; }
        delegate void ChangeTextDelegate(string newText);

        public MainLabelLogic(Label mainLbl)
        {
            this.MainLbl = mainLbl;
        }

        public void Update(AppState appState)
        {

            switch (appState)
            {
                case AppState.Loading:
                    this.ChangeText("Loading resources...");
                    return;

                case AppState.Setup:
                    this.ChangeText("Hang on while I set up here...");
                    return;

                case AppState.NoDevice:
                    this.ChangeText("Select a function to get started!");
                    return;

                case AppState.DeviceConnected:
                    this.ChangeText("Select an update option to continue...");
                    return;

                case AppState.Streaming:
                    this.ChangeText("Updating firmware...");
                    return;
            }
        }

        public void ChangeText(string newText)
        {
            if (this.MainLbl.Dispatcher.CheckAccess())
            {
                this.MainLbl.Content = newText;
            }
            else
            {
                this.MainLbl.Dispatcher.BeginInvoke(
                    new ChangeTextDelegate(ChangeText),
                    DispatcherPriority.Normal,
                    new object[] { newText });
            }
        }

        public void OnClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
