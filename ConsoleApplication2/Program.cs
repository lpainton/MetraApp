using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using FTD2XX_NET;

namespace Metra.Axxess
{
    class Program
    {
        static void Main(string[] args)
        {
            UInt32 ftdiDeviceCount = 0;
            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

            // Create new instance of the FTDI device class
            FTDICable myFtdiDevice = new FTDICable();

            // Determine the number of FTDI devices connected to the machine
            ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);
            // Check status
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                Console.WriteLine("Number of FTDI devices: " + ftdiDeviceCount.ToString());
                Console.WriteLine("");
            }
            else
            {
                // Wait for a key press
                Console.WriteLine("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // If no devices available, return
            if (ftdiDeviceCount == 0)
            {
                // Wait for a key press
                Console.WriteLine("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // Allocate storage for device info list
            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];

            // Populate our device list
            ftStatus = myFtdiDevice.GetDeviceList(ftdiDeviceList);

            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                for (UInt32 i = 0; i < ftdiDeviceCount; i++)
                {
                    Console.WriteLine("Device Index: " + i.ToString());
                    Console.WriteLine("Flags: " + String.Format("{0:x}", ftdiDeviceList[i].Flags));
                    Console.WriteLine("Type: " + ftdiDeviceList[i].Type.ToString());
                    Console.WriteLine("ID: " + String.Format("{0:x}", ftdiDeviceList[i].ID));
                    Console.WriteLine("Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId));
                    Console.WriteLine("Serial Number: " + ftdiDeviceList[i].SerialNumber.ToString());
                    Console.WriteLine("Description: " + ftdiDeviceList[i].Description.ToString());
                    Console.WriteLine("");
                }
            }

            //Bauds are 19200 or 115200
            /*OpenCommPort(115200, FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE, myFtdiDevice);
            myFtdiDevice.SetTimeouts(150, 100);
            myFtdiDevice.SetLatency(2);

            byte[] buffer = new byte[44];
            uint resp = 0;
            Queue<uint> baudRates = new Queue<uint>();
            baudRates.Enqueue(115200);
            baudRates.Enqueue(19200);*/
            myFtdiDevice.OpenPortForAxxess(9000);
            uint rate = 0;
            uint[] candidates = new uint[] { 19200, 115200 };

            while (rate == 0)
            {
                rate = myFtdiDevice.SearchBaudRate(candidates, 100);
                //Console.WriteLine("Writing intro...");
                //Console.WriteLine("Reading from cable...");
                /*uint resp = ReadFromPort(myFtdiDevice, buffer);
                if (resp >= 44)
                {
                    Console.WriteLine(resp.ToString());
                    foreach (byte b in buffer) { Console.Write("{0} ", b); }
                    foreach (byte b in buffer) { Console.Write("{0} ", Convert.ToChar(b)); }
                    Console.WriteLine();
                }*/
            }

            myFtdiDevice.CloseCommPort();
            Console.WriteLine("Baud rate of {0}", rate);
            //CloseCommPort(myFtdiDevice);
            Console.ReadKey();


            /*
            // Open first device in our list by serial number
            Console.WriteLine("Open by serial...");
            ftStatus = myFtdiDevice.OpenBySerialNumber(ftdiDeviceList[0].SerialNumber);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to open device (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // Set up device data parameters
            // Set Baud rate to 9600
            Console.WriteLine("Set baud rate...");
            //Bauds are 19200 or 115200
            ftStatus = myFtdiDevice.SetBaudRate(19200);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set Baud rate (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // Set data characteristics - Data bits, Stop bits, Parity
            Console.WriteLine("Set data characteristics...");
            ftStatus = myFtdiDevice.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set data characteristics (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // Set flow control - set RTS/CTS flow control
            Console.WriteLine("Set flow control...");
            ftStatus = myFtdiDevice.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_RTS_CTS, 0x11, 0x13);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set flow control (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // Set read timeout to 5 seconds, write timeout to infinite
            Console.WriteLine("Set read timeout...");
            ftStatus = myFtdiDevice.SetTimeouts(50, 0);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set timeouts (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            // Perform loop back - make sure loop back connector is fitted to the device
            // Write string data to the device
            /*Console.WriteLine("Write hello world...");
            string dataToWrite = "Hello world!";
            UInt32 numBytesWritten = 0;
            // Note that the Write method is overloaded, so can write string or byte array data
            ftStatus = myFtdiDevice.Write(dataToWrite, dataToWrite.Length, ref numBytesWritten);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to write to device (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return;
            }

            byte[] dataToWrite = new byte[44];
            dataToWrite[0] = 0x01;
            dataToWrite[1] = 0xF0;
            dataToWrite[2] = 0x20;
            dataToWrite[3] = 0x00;
            dataToWrite[4] = 0xEB;
            dataToWrite[5] = 0x04;
            UInt32 numBytesWritten = 0;
            UInt32 numBytesAvailable = 0;

            while(true)
            {
                ftStatus = myFtdiDevice.Write(dataToWrite, dataToWrite.Length, ref numBytesWritten);

                // Check the amount of data available to read
                // In this case we know how much data we are expecting, 
                // so wait until we have all of the bytes we have sent.
                Console.WriteLine("Check bytes to read...");

                ftStatus = myFtdiDevice.GetRxBytesAvailable(ref numBytesAvailable);
                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    // Wait for a key press
                    Console.WriteLine("Failed to get number of bytes available to read (error " + ftStatus.ToString() + ")");
                    Console.ReadKey();
                    return;
                }

                if (numBytesAvailable >= 44)
                {
                    // Now that we have the amount of data we want available, read it
                    string readData;
                    UInt32 numBytesRead = 0;
                    // Note that the Read method is overloaded, so can read string or byte array data
                    Console.WriteLine("Read data...");
                    ftStatus = myFtdiDevice.Read(out readData, 44, ref numBytesRead);
                    if (ftStatus != FTDI.FT_STATUS.FT_OK)
                    {
                        // Wait for a key press
                        Console.WriteLine("Failed to read data (error " + ftStatus.ToString() + ")");
                        Console.ReadKey();
                        return;
                    }
                    Console.WriteLine(readData);
                }
                Thread.Sleep(10);
            }

            // Close our device
            ftStatus = myFtdiDevice.Close();

            // Wait for a key press
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
            return;*/
        }

        public static bool OpenCommPort(uint baudRate, byte wordLen, byte stopBits, byte parity, FTDI myFtdiDevice)
        {
            FTDI.FT_STATUS ftStatus;

            Console.WriteLine("Open by index...");
            ftStatus = myFtdiDevice.OpenByIndex(0);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to open device (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return false;
            }

            Console.WriteLine("Resetting...");
            ftStatus = myFtdiDevice.ResetDevice();
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to reset device (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return false;
            }

            Console.WriteLine("Reset device...");
            ftStatus = myFtdiDevice.ResetDevice();
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to reset device (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return false;
            }

            // Set up device data parameters
            // Set Baud rate to 9600
            Console.WriteLine("Set baud rate...");
            //Bauds are 19200 or 115200
            ftStatus = myFtdiDevice.SetBaudRate(baudRate);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set Baud rate (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return false;
            }

            Console.WriteLine("Set data characteristics...");
            ftStatus = myFtdiDevice.SetDataCharacteristics(wordLen, stopBits, parity);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set data characteristics (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return false;
            }

            // Set flow control - set RTS/CTS flow control
            Console.WriteLine("Set flow control...");
            ftStatus = myFtdiDevice.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0x11, 0x19);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to set flow control (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return false;
            }

            Console.WriteLine("Purging...");
            ftStatus = myFtdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_TX | FTDI.FT_PURGE.FT_PURGE_RX);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to purge (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return false;
            }

            return true;
        }

        public static bool CloseCommPort(FTDI myFtdiDevice)
        {
            FTDI.FT_STATUS ftStatus;
            ftStatus = myFtdiDevice.Close();
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                Console.WriteLine("Failed to close (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return false;
            }
            return true;
        }

        public static uint WriteIntroToPort(FTDI myFtdiDevice)
        {
            //0x01, 0xF0, 0x10, 0x03, 0xA0, 0x01, 0x0F, 0x58, 0x04
            byte[] dataToWrite = new byte[9];
            dataToWrite[0] = 0x01;
            dataToWrite[1] = 0xF0;
            dataToWrite[2] = 0x10;
            dataToWrite[3] = 0x03;
            dataToWrite[4] = 0xA0;
            dataToWrite[5] = 0x01;
            dataToWrite[6] = 0x0F;
            dataToWrite[7] = 0x58;
            dataToWrite[8] = 0x04;
            UInt32 numBytesWritten = 0;
            
            myFtdiDevice.Write(dataToWrite, dataToWrite.Length, ref numBytesWritten);
            return numBytesWritten;
        }

        public static uint ReadFromPort(FTDI myFtdiDevice, byte[] buffer)
        {
            UInt32 numBytesRead = 0;
            myFtdiDevice.Read(buffer, 44, ref numBytesRead);
            return numBytesRead;
        }
    }
}
