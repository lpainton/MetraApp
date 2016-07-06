using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Metra.Axxess;

namespace MetraWPFBrowserApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Most of the application logic is handled by the MetraApp class object below.
        /// </summary>
        MetraApp App { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            App = MetraApp.GetInstance(this);
            App.Initialize();

            _mainLblDown = false;
        }

        //Main label click logic
        //Currently clicking the main label does nothing.
        bool _mainLblDown;
        private void MainLabel_MouseDown(object sender, MouseButtonEventArgs e) { _mainLblDown = true; }
        private void MainLabel_MouseLeave(object sender, MouseEventArgs e) { _mainLblDown = false; }
        private void MainLabel_MouseUp(object sender, MouseButtonEventArgs e) { if (_mainLblDown) MainLabel_Clicked(sender, new RoutedEventArgs()); }
        private void MainLabel_Clicked(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            LogManager.WriteToLog("Connect button clicked.");
            this.App.OnConnectButtonClick();
        }
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            LogManager.WriteToLog("Update button clicked.");
            this.App.OnUpdateButtonClick();
        }
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LogManager.WriteToLog("Load button clicked.");
            this.App.OnLoadButtonClick();
        }
        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            LogManager.WriteToLog("Configure button clicked.");
            this.App.OnConfigButtonClick();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.App.OnClose();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            this.App.OnAboutButtonClick();
        }


    }
}
