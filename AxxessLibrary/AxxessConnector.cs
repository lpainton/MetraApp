using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    /// <summary>
    /// An abstract factory class for Axxess devices.  
    /// Provides static methods to search for and return object representations of connected Axxess devices.
    /// </summary>
    public abstract class AxxessConnector
    {
        /* Product IDs and Vendor IDs for various board types
         * 1240 63493 USB_DATA_XFER_HID_WITHOUT_CHECKSUM
         * 1240 63301 USB_DATA_XFER_HID_WITH_CHECKSUM
         * 1115 8211 USB_DATA_XFER_HID_FOR_293
         * 1240 223 USB_DATA_XFER_CDC_MICROCHIP
         * 1027 24577 USB_DATA_XFER_CDC_FTDI
         * 1240 60 USB_DATA_XFER_HID_WITHOUT_CHECKSUM
         * 1155 22352 USB_DATA_XFER_HID_WITHOUT_CHECKSUM*/
        
        //HID w/o checksum
        const int HIDNoCheck1PID = 1240;
        const int HIDNoCheck1VID = 63493;
        const int HIDNoCheck2PID = 1240;
        const int HIDNoCheck2VID = 60;
        const int HIDNoCheck3PID = 1155;
        const int HIDNoCheck3VID = 22352;

        //HID with checksum
        const int HIDChecksumPID = 1240;
        const int HIDChecksumVID = 63301;

        //Misc
        const int HID293PID = 1115;
        const int HID293VID = 8211;
        const int CDCMICROPID = 1240;
        const int CDCMICROVID = 223;
        const int CDCFTDIPID = 1027;
        const int CDCFTDIVID = 24577;

        //Maximum search time for an FTDI device in milliseconds
        const int FTDIMaxSearch = 100;
        private static readonly uint[] FTDIBaudRates = { 19200, 115200 };

        #region Static Fields and Methods

        /// <summary>
        /// Searches for and resolves a connection with an Axxess device.
        /// Does a single pass on all known board types.
        /// </summary>
        /// 
        /// <returns>
        /// Returns the first found Axxess device in the hierarchy or null.
        /// </returns>
        /// 
        /// <example>
        /// Waiting for user to plug in a device.
        /// <code>
        /// IAxxessDevice dev = null;
        /// while(dev == null) {
        ///     dev = AxxessConnector.ResolveConnection();
        /// }
        /// </code></example>
        /// 
        /// <remarks>
        /// Please note that FTDI is always searched first due to the unusual behavior from FTDI boards which, on
        /// exititing the boot sequence, ignore further attempts to initiate boot mode.
        /// </remarks>
        /// 
        /// <seealso cref="IAxxessBoard"/>
        public static IAxxessBoard ResolveConnection()
        {
            IAxxessBoard device = null;
            
            device = (device == null) ? SearchForFTDI() : device;

            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HIDChecksumPID, HIDChecksumVID, typeof(AxxessHIDCheckBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HIDNoCheck1PID, HIDNoCheck1VID, typeof(AxxessHIDBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HIDNoCheck2PID, HIDNoCheck2VID, typeof(AxxessHIDBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HIDNoCheck3PID, HIDNoCheck3VID, typeof(AxxessHIDBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HID293PID, HID293VID, typeof(AxxessHID293Board)) : device;
            device = (device == null) ? (IAxxessBoard)CDCDevice.FindDevice(CDCMICROPID, CDCMICROVID, typeof(AxxessCDCBoard)) : device;

            return device;
        }

        /// <summary>
        /// Searches for a connected FTDI board using the FTDI .Net library. 
        /// </summary>
        /// <returns>An Axxess board object representing a connected FTDI device or null.</returns>
        /// <seealso cref="FTDICable"/>
        public static IAxxessBoard SearchForFTDI()
        {
            //Initialize the connection
            FTDICable myFtdiDevice = new FTDICable();

            uint devCount = 0;
            myFtdiDevice.GetNumberOfDevices(ref devCount);
            if (devCount != 1)
                return null;

            myFtdiDevice.OpenPortForAxxess(9000);
            uint rate = 0;

            rate = myFtdiDevice.SearchBaudRate(FTDIBaudRates, FTDIMaxSearch);           

            myFtdiDevice.CloseCommPort();

            return (rate > 0) ? new AxxessFTDIBoard(myFtdiDevice, rate) : null;
        }
        #endregion
    }
}
