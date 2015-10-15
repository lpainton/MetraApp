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
using Newtonsoft.Json;

namespace MetraWPFBrowserApp
{

    /// <summary>
    /// Interaction logic for ASCWWindow.xaml
    /// </summary>
    public partial class ASCWWindow : Window
    {
        IAxxessBoard AttachedDevice { get; set; }
        ASWCInfo WorkingSet { get; set; }



        public ASCWWindow(IAxxessBoard device)
        {
            InitializeComponent();

            AttachedDevice = device;
            WorkingSet = new ASWCInfo();

            this.DataContext = WorkingSet;
        }

        private void OnRendered(object sender, EventArgs e)
        {
            /*Thread.Sleep(1000);
            while (this.AttachedDevice.ASWCInformation == null)
            {
                //Console.WriteLine("ASCW packet send");
                this.AttachedDevice.SendASWCRequestPacket();
                Thread.Sleep(1000);
            }*/

            //UpdateASWCInfo(this.AttachedDevice.ASWCInformation);

        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveMap(this.WorkingSet);
        }

        private void SaveMap(ASWCInfo info)
        {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".mam"; // Default file extension
            dlg.Filter = "Metra ASCW Map (.mam)|*.mam"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;

                SaveMapToFile(info, filename);
            }
        }

        private void SaveMapToFile(ASWCInfo info, string filename)
        {
            string output = JsonConvert.SerializeObject(info);
            System.IO.File.WriteAllText(filename, output);
        }

        private void Load_Button_Click(object sender, RoutedEventArgs e)
        {
            LoadMap();
        }

        private void LoadMap()
        {
            // Configure load file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".mam"; // Default file extension
            dlg.Filter = "Metra ASCW Map (.mam)|*.mam"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;

                this.WorkingSet = LoadMapFromFile(filename);
                this.DataContext = this.WorkingSet;
            }
        }

        private ASWCInfo LoadMapFromFile(string filename)
        {
            string input = System.IO.File.ReadAllText(filename);
            return JsonConvert.DeserializeObject<ASWCInfo>(input);
        }



        /*private void UpdateASWCInfo(ASWCInfo info)
        {
            this.VersionLabel.Content = info.VersionString;

            //Radio Info
            this.RadioBox.SelectedIndex = Convert.ToInt32(info.RadioType);

            //Stalk info
            this.StalkCheck.IsChecked = info.StalkPresent;

            //Button remaps
            this.RemapCheckBox.IsChecked = info.ButtonRemapActive;

            //Mapping
            this.VolUpBox.SelectedIndex = info.VolumeUp;
            this.VolDownBox.SelectedIndex = info.VolumeDown;
            this.SeekUpBox.SelectedIndex = info.SeekUp;
            this.SeekDownBox.SelectedIndex = info.SeekDown;
            this.ModeSourceBox.SelectedIndex = info.ModeSource;
            this.MuteBox.SelectedIndex = info.Mute;
            this.PresetUpBox.SelectedIndex = info.PresetUp;
            this.PresetDownBox.SelectedIndex = info.PresetDown;
            this.PowerBox.SelectedIndex = info.Power;
            this.BandBox.SelectedIndex = info.Band;
            this.PlayEnterBox.SelectedIndex = info.PlayEnter;
            this.PTTBox.SelectedIndex = info.PTT;
            this.OnHookBox.SelectedIndex = info.OnHook;
            this.OffHookBox.SelectedIndex = info.OffHook;
            this.FanUpBox.SelectedIndex = info.FanUp;
            this.FanDownBox.SelectedIndex = info.FanDown;
            this.TempUpBox.SelectedIndex = info.TempUp;
            this.TempDownBox.SelectedIndex = info.TempDown;

            //Press and hold flags
            this.VolUpCheck.IsChecked = (info.PressHoldFlags3 & 0x01) == 0x01;
            this.VolDownCheck.IsChecked = (info.PressHoldFlags3 & 0x02) == 0x02;
            this.SeekUpCheck.IsChecked = (info.PressHoldFlags3 & 0x04) == 0x04;
            this.SeekDownCheck.IsChecked = (info.PressHoldFlags3 & 0x08) == 0x08;
            this.ModeSourceCheck.IsChecked = (info.PressHoldFlags3 & 0x10) == 0x10;
            this.MuteCheck.IsChecked = (info.PressHoldFlags3 & 0x20) == 0x20;
            this.PresetUpCheck.IsChecked = (info.PressHoldFlags3 & 0x40) == 0x40;
            this.PresetDownCheck.IsChecked = (info.PressHoldFlags3 & 0x80) == 0x80;
            this.PowerCheck.IsChecked = (info.PressHoldFlags2 & 0x01) == 0x01;
            this.BandCheck.IsChecked = (info.PressHoldFlags2 & 0x02) == 0x02;
            this.PlayEnterCheck.IsChecked = (info.PressHoldFlags2 & 0x04) == 0x04;
            this.PTTCheck.IsChecked = (info.PressHoldFlags2 & 0x08) == 0x08;
            this.OnHookCheck.IsChecked = (info.PressHoldFlags2 & 0x10) == 0x10;
            this.OffHookCheck.IsChecked = (info.PressHoldFlags2 & 0x20) == 0x20;
            this.FanUpCheck.IsChecked = (info.PressHoldFlags2 & 0x40) == 0x40;
            this.FanDownCheck.IsChecked = (info.PressHoldFlags2 & 0x80) == 0x80;
            this.TempUpCheck.IsChecked = (info.PressHoldFlags1 & 0x01) == 0x01;
            this.TempDownCheck.IsChecked = (info.PressHoldFlags1 & 0x02) == 0x02;

            //PH Mapping
            this.PHVolUpBox.SelectedIndex = info.PressHoldVolumeUp;
            this.PHVolDownBox.SelectedIndex = info.PressHoldVolumeDown;
            this.PHSeekUpBox.SelectedIndex = info.PressHoldSeekUp;
            this.PHSeekDownBox.SelectedIndex = info.PressHoldSeekDown;
            this.PHModeSourceBox.SelectedIndex = info.PressHoldModeSource;
            this.PHMuteBox.SelectedIndex = info.PressHoldMute;
            this.PHPresetUpBox.SelectedIndex = info.PressHoldPresetUp;
            this.PHPresetDownBox.SelectedIndex = info.PressHoldPresetDown;
            this.PHPowerBox.SelectedIndex = info.PressHoldPower;
            this.PHBandBox.SelectedIndex = info.PressHoldBand;
            this.PHPlayEnterBox.SelectedIndex = info.PressHoldPlayEnter;
            this.PHPTTBox.SelectedIndex = info.PressHoldPTT;
            this.PHOnHookBox.SelectedIndex = info.PressHoldOnHook;
            this.PHOffHookBox.SelectedIndex = info.PressHoldOffHook;
            this.PHFanUpBox.SelectedIndex = info.PressHoldFanUp;
            this.PHFanDownBox.SelectedIndex = info.PressHoldFanDown;
            this.PHTempUpBox.SelectedIndex = info.PressHoldTempUp;
            this.PHTempDownBox.SelectedIndex = info.PressHoldTempDown;
            
        }*/

        /*private void StalkClicked(object sender, RoutedEventArgs e)
        {
            StalkLeft.IsEnabled = StalkCheck.IsChecked.Value;
            StalkLeft.IsEnabled = StalkCheck.IsChecked.Value;
        }*/
    }
}
