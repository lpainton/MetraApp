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
        public ASWCInfo WorkingSet { get; set; }
        delegate void ContextDelegate();

        public ASCWWindow(IAxxessBoard device)
        {
            InitializeComponent();

            AttachedDevice = device;
            WorkingSet = new ASWCInfo();

            this.DataContext = WorkingSet;
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
                this.DataContext = this.WorkingSet;
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

                MessageBox.Show("Timed out while reading ASCW info.  Please disconnect device and try again!");
            }
            finally
            {
                this.AttachedDevice.RemoveASWCInfoEvent(handler);
                this.DataContext = this.WorkingSet;
            }
        }

        private void Write_Button_Click(object sender, RoutedEventArgs e)
        {
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
                this.AttachedDevice.SendASWCMappingPacket(this.WorkingSet);
                bool set = waitHandle.Wait(15000);
                if (!set) throw new TimeoutException();

                //Read packet for result 
                string msg = String.Empty;
                switch (packet[8])
                {
                    //0x00 – ALL DATA IS GOOD AND RECORDED
                    case 0x00:
                        msg = "Device configuration successful!";
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
            }
            catch (TimeoutException)
            {
                MessageBox.Show("Timed out while writing ASCW info.  Please disconnect device and try again!");
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
        }
        public void OnDeviceRemoved(object sender, EventArgs e)
        {
            this.AttachedDevice.Dispose();
            this.AttachedDevice = null;
            this.WorkingSet = new ASWCInfo();
            this.SetContextToWorkingSet();
        }
        public void SetContextToWorkingSet()
        {
            if (this.Dispatcher.CheckAccess())
            {
                this.DataContext = this.WorkingSet;
            }
            else
            {
                this.Dispatcher.BeginInvoke(
                    new ContextDelegate(SetContextToWorkingSet),
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new object[] { });
            }
        }
    }
}
