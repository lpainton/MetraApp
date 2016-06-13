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
    public partial class ASWCWindow : Window
    {
        IAxxessBoard AttachedDevice { get; set; }
        ASWCInfo _workingSet;
        public ASWCInfo WorkingSet 
        { 
            get
            {
                return _workingSet;
            }
            set
            {
                if (_workingSet != value)
                {
                    _workingSet = value;
                }
            }
        }
        List<SectionChanged> _changedSections;
        public void MarkSectionChanged(SectionChanged section)
        {
            if (!_changedSections.Contains(section))
            {
                LogManager.WriteToLog(section.ToString() + " setting was changed by user.");
                _changedSections.Add(section);
            }
        }
        public void ClearChangedSections()
        {
            _changedSections.Clear();
        }

        delegate void ContextDelegate();

        public ASWCWindow(IAxxessBoard device)
        {
            InitializeComponent();

            AttachedDevice = device;
            _changedSections = new List<SectionChanged>();
            WorkingSet = new ASWCInfo();

            //this.DataContext = WorkingSet;
            SetContextToWorkingSet();
        }

        private void OnRendered(object sender, EventArgs e)
        {

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
                //this.DataContext = this.WorkingSet;
                SetContextToWorkingSet();
            }
        }

        private ASWCInfo LoadMapFromFile(string filename)
        {
            string input = System.IO.File.ReadAllText(filename);
            return JsonConvert.DeserializeObject<ASWCInfo>(input);
        }

        private void Read_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConnectWithDevice();
            }
            catch (AxxessDeviceException)
            {
                return;
            }

            //Prep for read
            this.WorkingSet = null;
            ManualResetEventSlim waitHandle = new ManualResetEventSlim();
            ASWCInfoHandler handler = (send, evargs) =>
            {
                this.WorkingSet = ((ASWCEventArgs)evargs).Info;
                waitHandle.Set();
            };
            this.AttachedDevice.AddASWCInfoEvent(handler);

            int retries = 3;
            try
            {
                while ((retries--) > 0)
                {
                    this.AttachedDevice.SendASWCRequestPacket();
                    bool set = waitHandle.Wait(3000);
                    if (set) break;
                    else waitHandle.Reset();
                }
                if (retries <= 0) throw new TimeoutException();
            }
            catch (TimeoutException)
            {
                string msg = "Timed out while reading ASCW info.  Please disconnect device and try again!";
                MessageBox.Show(msg);
                LogManager.WriteToLog(msg);
            }
            catch (NotImplementedException ex)
            {
                string msg = "Button configuration is not enabled for this type of device.  Please plug in a different board.";
                MessageBox.Show(msg);
                LogManager.WriteToLog(msg);
            }
            finally
            {
                this.AttachedDevice.RemoveASWCInfoEvent(handler);
                SetContextToWorkingSet();
            }
        }

        private void Write_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_changedSections.Count == 0)
            {
                MessageBox.Show("Error: No changes were made to the previous configuration!");
                return;
            }

            try 
            {
                ConnectWithDevice(); 
            }
            catch (AxxessDeviceException)
            {
                return;
            }

            //Thread.Sleep(1000);

            //Prep for read
            ManualResetEventSlim waitHandle = new ManualResetEventSlim();
            byte[] packet = null;
            ASWCConfirmHandler handler = (send, evargs) =>
            {
                packet = ((PacketEventArgs)evargs).Packet;
                waitHandle.Set();
            };
            this.AttachedDevice.AddASWCConfimEvent(handler);

            try
            {
                this.AttachedDevice.SendASWCMappingPacket(this.WorkingSet, _changedSections);
                bool set = waitHandle.Wait(15000);
                if (!set && packet == null) throw new TimeoutException();

                //Read packet for result 
                string msg = String.Empty;
                byte resp = packet[8];
                switch (resp)
                {
                    //0x00 – ALL DATA IS GOOD AND RECORDED
                    case 0x00:
                        msg = "Device configuration successful!";
                        ClearChangedSections();
                        break;

                    //0xA2 – RADIO TYPE NUMBER FAILURE 
                    case 0xA2:
                        msg = "Error: Radio type number failure.";
                        break;

                    //0xA3 – COMMUNICAITON TYPE FAILURE 
                    case 0xA3:
                        msg = "Error: Communication type failure.";
                        break;

                    //0xA4 – STALK FLAG FAILURE
                    case 0xA4:
                        msg = "Error: Stalk flag failure.";
                        break;

                    //0xA5 – STALK FLAG ORIENTATION FAILURE
                    case 0xA5:
                        msg = "Error: Stalk flag orientation failure.";
                        break;

                    //0xA6 – PRESS/HOLD FLAG FAILURE 
                    case 0xA6:
                        msg = "Error: Press/hold flag failure.";
                        break;

                    //0xA7 – PRESS/HOLD BUTTON FAILURE 
                    case 0xA7:
                        msg = "Error: Press/hold button failure.";
                        break;

                    //0xA8 – BUTTON REMAP FLAG FAILURE 
                    case 0xA8:
                        msg = "Error: Button remap flag failure.";
                        break;

                    default:
                        msg = "Error: An unknown error occurred.";
                        break;
                }

                MessageBox.Show(msg);
                LogManager.WriteToLog("ASWC " + msg);
            }
            catch (TimeoutException ex)
            {
                String msg = "Timed out while writing ASCW info.";
                MessageBox.Show(msg + " Please disconnect device and try again!");
                LogManager.WriteToLog(msg);
            }
            catch (NotImplementedException ex)
            {
                string msg = "Button configuration is not enabled for this type of device.";
                MessageBox.Show(msg + " Please plug in a different board.");
                LogManager.WriteToLog(msg);
            }
            catch (Exception ex)
            {
                string msg = "An unknown error occured!";
                MessageBox.Show(msg);
                LogManager.WriteToLog(ex.Message);
                this.Close();
            }
            finally
            {
                this.AttachedDevice.RemoveASWCConfimEvent(handler);
            }
        }

        private void ConnectWithDevice()
        {
            if (this.AttachedDevice == null)
            {
                ConnectionForm c = new ConnectionForm();
                c.ShowDialog();
                this.AttachedDevice = c.Device;
                c.Dispose();
            }
            if (this.AttachedDevice == null)
            {
                MessageBox.Show("No device was found.  Please plug a device into the computer before retrying.");
                LogManager.WriteToLog("No device was found.");
                throw new AxxessDeviceException();
            }
            else
            {
                OnDeviceConnect();
            }
        }
        public void OnDeviceConnect()
        {
            this.AttachedDevice.AddRemovedEvent(OnDeviceRemoved);
            LogManager.WriteToLog("Connected to " + this.AttachedDevice.Type);
        }
        public void OnDeviceRemoved(object sender, EventArgs e)
        {
            try
            {
                this.AttachedDevice.Dispose();
            }
            catch (NullReferenceException)
            {
                LogManager.WriteToLog("Null reference exception on device removal.");
            }
            finally
            {
                this.AttachedDevice = null;
                this.WorkingSet = new ASWCInfo();
                this.SetContextToWorkingSet();
            }
        }
        public void SetContextToWorkingSet()
        {
            if (this.Dispatcher.CheckAccess())
            {
                this.DataContext = this.WorkingSet;
                ClearChangedSections();
            }
            else
            {
                this.Dispatcher.BeginInvoke(
                    new ContextDelegate(SetContextToWorkingSet),
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new object[] { });
            }
        }

        private void SpeedChanged(object sender, SelectionChangedEventArgs e)
        {
            MarkSectionChanged(SectionChanged.SpeedControl);
        }
        private void RadioChanged(object sender, SelectionChangedEventArgs e)
        {
            MarkSectionChanged(SectionChanged.Radio);
        }
        private void StalkOptionsChanged(object sender, SelectionChangedEventArgs e)
        {
            //Console.WriteLine("Stalk changed to {0}", (sender as ComboBox).SelectedItem);
            MarkSectionChanged(SectionChanged.Stalk);
            if ((string)((sender as ComboBox).SelectedItem) != "Unknown")
                StalkCheck.IsChecked = true;
        }
        private void StalkChanged(object sender, RoutedEventArgs e)
        {
            MarkSectionChanged(SectionChanged.Stalk);
        } 
        private void ButtonChanged(object sender, SelectionChangedEventArgs e)
        {
            MarkSectionChanged(SectionChanged.Buttons);
            if ((string)((sender as ComboBox).SelectedItem) != "Default")
                RemapCheckBox.IsChecked = true;
        }
        private void RemapChanged(object sender, RoutedEventArgs e)
        {
            MarkSectionChanged(SectionChanged.Buttons);
        }
        private void PHChanged(object sender, RoutedEventArgs e)
        {
            MarkSectionChanged(SectionChanged.PressHold);
        }
        private void PHBChanged(object sender, SelectionChangedEventArgs e)
        {
            MarkSectionChanged(SectionChanged.PressHold);
            ComboBox box = (ComboBox)sender;
            if (box.SelectedItem.ToString() != "Default")
            {
                //Find button selected
                string button = WorkingSet.ButtonList.First(item => box.Name.Contains(item));
                //Console.WriteLine(button);
                
            }
        }
    }
}
