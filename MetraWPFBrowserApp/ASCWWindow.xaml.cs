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
using System.Windows.Shapes;
using System.Threading;
using Metra.Axxess;

namespace MetraWPFBrowserApp
{
    public enum ASCWButtonTypes
    {
        Default,
        VolumeUp,
        VolumeDown,
        SeekUp,
        SeekDown,
        ModeOrSource,
        Mute,
        PresetUp,
        PresetDown,
        Power,
        Band,
        PlayOrEnter,
        PTT,
        OnHook,
        OffHook,
        FanUp,
        FanDown,
        TemperatureUp,
        TemperatureDown
    };

    /// <summary>
    /// Interaction logic for ASCWWindow.xaml
    /// </summary>
    public partial class ASCWWindow : Window
    {
        IAxxessBoard AttachedDevice { get; set; }

        public string[] ButtonList { get { return _blist.ToArray(); } }
        private List<string> _blist;

        public ASCWWindow(IAxxessBoard device)
        {
            InitializeComponent();

            AttachedDevice = device;

            this.DataContext = this;

            this.VersionLabel.Content = "Loading";

            _blist = new List<string>();
            foreach (string bt in Enum.GetNames(typeof(ASCWButtonTypes)))
            {
                _blist.Add(bt);
            }
            
        }

        private void OnRendered(object sender, EventArgs e)
        {
            Thread.Sleep(1000);
            while (this.AttachedDevice.ASWCInformation == null)
            {
                //Console.WriteLine("ASCW packet send");
                this.AttachedDevice.SendASWCRequestPacket();
                Thread.Sleep(1000);
            }

            UpdateASWCInfo(this.AttachedDevice.ASWCInformation);

        }

        private void UpdateASWCInfo(ASWCInfo info)
        {
            this.VersionLabel.Content = info.VersionString;

            //Radio Info

            //Stalk info
            this.StalkCheck.IsChecked = info.StalkPresent;
            this.StalkLeft.IsEnabled = info.StalkPresent;
            this.StalkRight.IsEnabled = info.StalkPresent;
            switch (info.StalkOrientation)
            {
                case 0x00:

                    break;

                case 0x01:
                    break;

                case 0x02:
                    break;

            }

            //Button remaps
            
        }

        private void StalkClicked(object sender, RoutedEventArgs e)
        {
            StalkLeft.IsEnabled = StalkCheck.IsChecked.Value;
            StalkLeft.IsEnabled = StalkCheck.IsChecked.Value;
        }
    }
}
