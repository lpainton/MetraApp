using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Management;

namespace Metra.Axxess
{
    //Settings: baud=115200 parity=N data=8 stop=1
    //Intro? 01 F0 10 03 A0 01 0F 58 04
    //Ready: 01 F0 20 00 EB 04
    class Program
    {
        public static void Main()
        {
            /*byte[] barr = new byte[] { 0x01, 0x0F, 0x10, 0x16, 0x1A, 0x0A, 0x43, 0x57, 0x49, 0x32, 0x35, 0x37, 0x32, 0x36, 0x36, 0x20, 0x20, 
                0x20, 0x1C, 0x09, 0x33, 0x30, 0x20, 0xFF, 0xFF, 0x31, 0x30, 0x35, 0x00, 0x00, 0x43, 0x57, 0x49, 0x32, 0x35, 0x37, 0x32, 0x36, 0x36,
                0x20, 0x20, 0x20, 0xFF, 0x04 };
            List<char> blist = new List<char>();

            for (int i=0; i<barr.Length; i++)
            {
                Console.WriteLine("{0} -> {1}", i, Convert.ToChar(barr[i]));
            }
           
            Console.ReadKey();*/

            AxxessHIDBoard dev = null;
            while (dev == null)
                dev = (AxxessHIDBoard)AxxessConnector.InitiateConnection();

            Thread.Sleep(100);

            while (true)
            {
                dev.SendIntroPacket();
                Thread.Sleep(50);
            }
        }

    }
}
