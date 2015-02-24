using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;


namespace MetraApp
{
    class Program
    {
        static void Main(string[] args)
        {
            const int VID = 1240;
            const int PID = 63301;
            const string firmwarePath = "../../data/257291_268_ASWC-USB_V320_wencrypt.hex";

            //Console.Out.Write("Test compile\n");
            HIDChecksumBoard testdev = null;
            while (true)
            {
                Console.Out.Write("Fetching device...\n");
                testdev = (HIDChecksumBoard)HIDDevice.FindDevice(VID, PID, typeof(HIDChecksumBoard));
                String msg = (testdev == null) ? "Device not found!" : "Success!";
                Console.Out.WriteLine(msg);

                if (testdev == null)
                {
                    DialogResult dr2 = MessageBox.Show("Device was not found.  Please plug in device and retry.", "Metra App for Windows", MessageBoxButtons.RetryCancel);
                    if (dr2.Equals(DialogResult.Cancel))
                        return;
                }
                else
                {
                    break;
                }
            }
            

            //Preparsed data
            Console.Out.WriteLine("Input report length: " + testdev.InputReportLength);
            Console.Out.WriteLine("Output report length: " + testdev.OutputReportLength);

            //Listen to board
            int counter = 0;
            while (testdev.ProductID == 0)
            {
                try
                {
                    Console.Out.WriteLine("Sending intro packet...");
                    testdev.SendIntroPacket();
                    System.Threading.Thread.Sleep(200);
                    counter++;
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e.Message.ToString());
                    System.Threading.Thread.Sleep(1000);
                }

                if (counter == 10)
                {
                    MessageBox.Show("Device not responding.  Please try disconnecting the device and restarting this application.", "Metra App for Windows");
                    return;
                }
            }

            ThreadStart work = new ThreadStart(testdev.IntroSpam);
            Thread nThread = new Thread(work);
            nThread.Start();

            Console.Out.WriteLine("Product ID: " + testdev.ProductID);
            Console.Out.WriteLine("Firmware Version: " + testdev.AppFirmwareVersion);

            DialogResult dr = MessageBox.Show("Update to firmware version 3.20?", "Metra App for Windows", MessageBoxButtons.YesNo);

            if (dr.Equals(DialogResult.Yes))
            {
                testdev.UpdateAppFirmware(firmwarePath);
            }         
        }
    }
}
