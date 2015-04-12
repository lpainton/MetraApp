using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    abstract class AxxessConnector
    {
        /* 1240 63493 USB_DATA_XFER_HID_WITHOUT_CHECKSUM
         * 1240 63301 USB_DATA_XFER_HID_WITH_CHECKSUM
         * 1115 8211 USB_DATA_XFER_HID_FOR_293
         * 1240 223 USB_DATA_XFER_CDC_MICROCHIP
         * 1027 24577 USB_DATA_XFER_CDC_FTDI
         * 1240 60 USB_DATA_XFER_HID_WITHOUT_CHECKSUM
         * 1155 22352 USB_DATA_XFER_HID_WITHOUT_CHECKSUM*/
        
        //HID w/o checksum
        static const int HIDNoCheck1PID = 1240;
        static const int HIDNoCheck1VID = 63493;
        static const int HIDNoCheck2PID = 1240;
        static const int HIDNoCheck2VID = 60;
        static const int HIDNoCheck3PID = 1155;
        static const int HIDNoCheck3VID = 22352;

        //HID with checksum
        static const int HIDChecksumPID = 1240;
        static const int HIDChecksumVID = 63301;

        //Misc
        static const int HID293PID = 1115;
        static const int HID293VID = 8211;
        static const int CDCMICROPID = 1240;
        static const int CDCMICROVID = 223;
        static const int CDCFTDIPID = 1027;
        static const int CDCFTDIVID = 24577;

        #region Statics
        public static IAxxessBoard InitiateConnection()
        {
            IAxxessBoard device = (IAxxessBoard)HIDDevice.FindDevice(HIDChecksumPID, HIDChecksumVID, typeof(AxxessHIDCheckBoard));
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HIDNoCheck1PID, HIDNoCheck1VID, typeof(AxxessHIDCheckBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HIDNoCheck2PID, HIDNoCheck2VID, typeof(AxxessHIDCheckBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HIDNoCheck3PID, HIDNoCheck3VID, typeof(AxxessHIDCheckBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(HID293PID, HID293VID, typeof(AxxessHIDCheckBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(CDCMICROPID, CDCMICROVID, typeof(AxxessHIDCheckBoard)) : device;
            device = (device == null) ? (IAxxessBoard)HIDDevice.FindDevice(CDCFTDIPID, CDCFTDIVID, typeof(AxxessHIDCheckBoard)) : device;

            return device;
        }
        #endregion
    }
}
