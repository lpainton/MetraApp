using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Management;

namespace Metra.Axxess
{
    //0, 1, 15, 32, 0, 204, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
    //0, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
    class Program
    {
        public static void Main()
        {
            /*byte[] array = new byte[] { 0x00, 0x55, 0xB0, 0x09, 0x01, 0xF0, 0xA0, 0x03, 0x10, 0x01, 0x00, 0x57, 0x04,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00 };*/

            byte[] array = new byte[] { 0x01, 0xF0, 0xA0, 0x03, 0x10, 0x01, 0x00, 0x57, 0x04 };

            //Console.WriteLine(array.Length);

            IAxxessBoard dev = null;
            while (dev == null)
            {
                dev = AxxessConnector.InitiateConnection();
                Thread.Sleep(100);
            }

            byte[] newArray = array;
            Console.WriteLine("Sending intro packet!");
            dev.SendIntroPacket();
            //newArray[64] = dev.CalculateChecksum(array);
            //Console.WriteLine(newArray.Length);

            //Console.WriteLine(dev is AxxessHIDCheckBoard);
            //Console.WriteLine(dev is AxxessHIDBoard);

            Thread.Sleep(1000);

            while (dev.ASWCInformation == null)
            {
                Thread.Sleep(1000);
                //Console.ReadKey();
                Console.WriteLine("Sending ASCW request.");
                //dev.SendRawPacket(newArray);
                dev.SendASWCRequestPacket();
            }

            Console.WriteLine(dev.ASWCInformation.ToString());
            Console.ReadKey();
        }

    }
}
