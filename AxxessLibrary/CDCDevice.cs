using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Management;

namespace Metra.Axxess
{
    public class CDCAsynchReadResult : IAsyncResult
    {
        public object State { get; set; }
        public bool IsCompleted { get; set; }
        public bool TestMode { get; set; }

        public CDCAsynchReadResult()
        {
            State = null;
            IsCompleted = false;
        }

        object IAsyncResult.AsyncState
        {
            get { return this.State; }
        }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get { throw new NotImplementedException(); }
        }

        bool IAsyncResult.CompletedSynchronously
        {
            get { throw new NotImplementedException(); }
        }

        bool IAsyncResult.IsCompleted
        {
            get { return this.IsCompleted; }
        }
    }

    public class CDCDevice : Win32Usb
    {
        public SerialPort Port { get { return _serialPort; } }
        SerialPort _serialPort;
        public bool StopRead { get; set; }
        public bool TestMode { get; set; }

        public int ReadTimeout
        {
            get 
            { 
                return _serialPort.ReadTimeout; 
            }
            set 
            {
                _serialPort.ReadTimeout = value;
            }
        }
        public int WriteTimeout
        {
            get
            {
                return _serialPort.WriteTimeout;
            }
            set
            {
                _serialPort.WriteTimeout = value;
            }
        }

        public CDCDevice(int baud, Parity parity, int dataBits, StopBits stop, Handshake hs,
            int readTimeOut, int writeTimeOut, bool testMode = false)
        {
            TestMode = testMode;

            _serialPort = new SerialPort();

            //Settings
            _serialPort.PortName = SearchForPort();
            _serialPort.BaudRate = baud;
            _serialPort.Parity = parity;
            _serialPort.DataBits = dataBits;
            _serialPort.StopBits = stop;
            _serialPort.Handshake = hs;

            _serialPort.ReadTimeout = readTimeOut;
            _serialPort.WriteTimeout = WriteTimeout;
        }

        public CDCDevice(string name, int baud, Parity parity, int dataBits, StopBits stop, Handshake hs,
            int readTimeOut, int writeTimeOut, bool testMode = false)
        {
            TestMode = testMode;
            
            _serialPort = new SerialPort();

            //Settings
            _serialPort.PortName = name;
            _serialPort.BaudRate = baud;
            _serialPort.Parity = parity;
            _serialPort.DataBits = dataBits;
            _serialPort.StopBits = stop;
            _serialPort.Handshake = hs;

            _serialPort.ReadTimeout = readTimeOut;
            _serialPort.WriteTimeout = WriteTimeout;

            //this.Initialize();
        }

        protected string SearchForPort()
        {
            SelectQuery sq = new SelectQuery("Win32_PnPEntity");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(sq);

            Log.Write("Port not initialized, searching for port...", LogMode.Verbose);
            foreach (ManagementObject port in searcher.Get())
            {
                string name = port["Name"].ToString();
                if (name.Contains("COM") && port["Manufacturer"].ToString().Contains("Microchip"))
                {
                    List<char> n = new List<char>(name.ToCharArray());
                    string portName = "COM" + new string(n.FindAll(c => char.IsDigit(c)).ToArray());
                    Log.Write("CDC device found at " + portName, LogMode.Verbose);
                    return portName;
                }
            }

            Log.Write("CDC COM port not found!", LogMode.Verbose);
            throw new System.IO.IOException("No CDC COM port identified!");
        }

        protected virtual void Initialize()
        {
            this.StopRead = false;

            while (!_serialPort.IsOpen)
            {
                Log.Write("Openning serial port...", LogMode.Verbose);
                try
                {
                    _serialPort.Open();
                    Log.Write("Successfully openned port!", LogMode.Verbose);
                }
                catch (System.IO.IOException e)
                {
                    Log.Write("Error: " + e.Message);
                }
                Thread.Sleep(100);
            }

            Log.Write("Beginning read thread...", LogMode.Verbose);
            try
            {
                BeginAsyncRead();
            }
            catch (System.IO.IOException e)
            {
                Log.Write("Error: " + e.Message);
            }
        }

        protected virtual void HandleDataReceived(byte[] packet) { }

        protected virtual void HandleDeviceRemoved() 
        {
            OnDeviceRemoved(this, new EventArgs());
            this.Dispose();
        }
        public event EventHandler OnDeviceRemoved;

        public void BeginRead(AsyncCallback callback)
        {
            CDCAsynchReadResult result = new CDCAsynchReadResult();
            Thread ReadWorker = new Thread(o =>
            {
                CDCAsynchReadResult res = (CDCAsynchReadResult)o;
                res.IsCompleted = false;
                List<byte> buffer = new List<byte>();

                try
                {
                    while (true)
                    {
                        buffer.Add(this.Read());
                    }
                }
                catch (TimeoutException)
                {
                    res.IsCompleted = (buffer.Count > 0);
                    res.State = buffer.ToArray();
                }
                callback((IAsyncResult)res);
            });
            ReadWorker.Start(result);
        }
        private void BeginAsyncRead()
        {
            if (_serialPort.IsOpen)
            {
                BeginRead(ReadCompleted);
            }
            else
            {
                throw new System.IO.IOException("Port was not open for read operation.");
            }
        }
        public void ReadCompleted(IAsyncResult result)
        {
            try
            {
                if (result.IsCompleted)
                {
                    HandleDataReceived((byte[])result.AsyncState);
                }
            }
            finally
            {
                if (!StopRead)
                {
                    BeginAsyncRead();
                }
            }
        }
        public byte Read()
        {
            try
            {
                byte b = Convert.ToByte(_serialPort.ReadByte());
                Log.Write(b.ToString() + " " + Convert.ToInt32(b).ToString() + " " + Convert.ToChar(b).ToString());
                return b;
            }
            catch (System.IO.IOException)
            {
                this.Dispose();
            }
            catch (TimeoutException e)
            {
                throw e;
            }
            catch (InvalidOperationException)
            {
                this.Dispose();
            }
            return 0x00;
        }

        public void Write(byte[] packet)
        {
            if (this._serialPort.IsOpen)
            {
                try
                {
                    this._serialPort.Write(packet, 0, packet.Length);
                }
                catch (System.IO.IOException)
                {
                    HandleDeviceRemoved();
                }
            }
            else
            {
                throw new System.IO.IOException("Port was not open for write operation.");
            }
        }

        public void Dispose()
        {
            this.StopRead = true;
            try
            {
                if (this._serialPort.IsOpen)
                {
                    this._serialPort.Close();
                }
            }
            catch (System.IO.IOException)
            {
                Log.Write("Error trying to close port.");
            }
            finally
            {
                this._serialPort.Dispose();
            }

        }

        #region Test
        /*void TestConsoleWrite(string s)
        {
            Util.TestConsoleWrite(this.TestMode, s);
        }*/
        public static void TestConnect()
        {

        }
        #endregion

        #region Public static
        /// <summary>
        /// Helper method to return the device path given a DeviceInterfaceData structure and an InfoSet handle.
        /// Used in 'FindDevice' so check that method out to see how to get an InfoSet handle and a DeviceInterfaceData.
        /// </summary>
        /// <param name="hInfoSet">Handle to the InfoSet</param>
        /// <param name="oInterface">DeviceInterfaceData structure</param>
        /// <returns>The device path or null if there was some problem</returns>
        private static string GetDevicePath(IntPtr hInfoSet, ref DeviceInterfaceData oInterface)
        {
            uint nRequiredSize = 0;
            // Get the device interface details
            if (!SetupDiGetDeviceInterfaceDetail(hInfoSet, ref oInterface, IntPtr.Zero, 0, ref nRequiredSize, IntPtr.Zero))
            {
                DeviceInterfaceDetailData oDetail = new DeviceInterfaceDetailData();
                oDetail.Size = 5;	// hardcoded to 5! Sorry, but this works and trying more future proof versions by setting the size to the struct sizeof failed miserably. If you manage to sort it, mail me! Thx
                if (SetupDiGetDeviceInterfaceDetail(hInfoSet, ref oInterface, ref oDetail, nRequiredSize, ref nRequiredSize, IntPtr.Zero))
                {
                    return oDetail.DevicePath;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a device given its PID and VID
        /// </summary>
        /// <param name="nVid">Vendor id for device (VID)</param>
        /// <param name="nPid">Product id for device (PID)</param>
        /// <param name="oType">Type of device class to create</param>
        /// <returns>A new device class of the given type or null</returns>
        public static CDCDevice FindDevice(int nVid, int nPid, Type oType)
        {
            string strPath = string.Empty;
            string strSearch = string.Format("vid_{0:x4}&pid_{1:x4}", nVid, nPid); // first, build the path search string
            Guid gHid;
            HidD_GetHidGuid(out gHid);	// next, get the GUID from Windows that it uses to represent the HID USB interface
            IntPtr hInfoSet = SetupDiGetClassDevs(ref gHid, null, IntPtr.Zero, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);	// this gets a list of all HID devices currently connected to the computer (InfoSet)
            try
            {
                DeviceInterfaceData oInterface = new DeviceInterfaceData();	// build up a device interface data block
                oInterface.Size = Marshal.SizeOf(oInterface);
                // Now iterate through the InfoSet memory block assigned within Windows in the call to SetupDiGetClassDevs
                // to get device details for each device connected
                int nIndex = 0;
                while (SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref gHid, (uint)nIndex, ref oInterface))	// this gets the device interface information for a device at index 'nIndex' in the memory block
                {
                    string strDevicePath = GetDevicePath(hInfoSet, ref oInterface);	// get the device path (see helper method 'GetDevicePath')
                    if (strDevicePath.IndexOf(strSearch) >= 0)	// do a string search, if we find the VID/PID string then we found our device!
                    {
                        CDCDevice oNewDevice = (CDCDevice)Activator.CreateInstance(oType);	// create an instance of the class for this device
                        oNewDevice.Initialize();	// initialise it with the device path
                        return oNewDevice;	// and return it
                    }
                    nIndex++;	// if we get here, we didn't find our device. So move on to the next one.
                }
            }
            finally
            {
                // Before we go, we have to free up the InfoSet memory reserved by SetupDiGetClassDevs
                SetupDiDestroyDeviceInfoList(hInfoSet);
            }
            return null;	// oops, didn't find our device
        }
        #endregion
    }
}
