using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    public abstract class AxxessConnector
    {
        /* 1240 63493 USB_DATA_XFER_HID_WITHOUT_CHECKSUM
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

        const int FTDIMaxSearch = 100;
        private static readonly uint[] FTDIBaudRates = { 19200, 115200 };

        #region Statics
        public static IAxxessBoard InitiateConnection()
        {
            IAxxessBoard device = null;
            
            device = (device == null) ? SearchForFTDI() : device;

            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HIDChecksumPID, HIDChecksumVID, typeof(AxxessHIDCheckBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HIDNoCheck1PID, HIDNoCheck1VID, typeof(AxxessHIDBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HIDNoCheck2PID, HIDNoCheck2VID, typeof(AxxessHIDBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HIDNoCheck3PID, HIDNoCheck3VID, typeof(AxxessHIDBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HID293PID, HID293VID, typeof(AxxessHIDCheckBoard)) : device;
            //device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(CDCMICROPID, CDCMICROVID, typeof(AxxessHIDCheckBoard)) : device;
            //device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(CDCFTDIPID, CDCFTDIVID, typeof(AxxessHIDCheckBoard)) : device;

            return device;
        }

        public static IAxxessBoard SearchForFTDI()
        {
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
