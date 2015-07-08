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
            AxxessCDCBoard board = null;

            while (board == null)
            {
                board = (AxxessCDCBoard)AxxessConnector.InitiateConnection();
            }

            board.StopRead = true;

            while (true)
            {
                board.SendIntroPacket();
                string s = String.Empty;
                try
                {
                    /*int bytes = board.Port.BytesToRead;
                    byte[] buffer = new byte[bytes];
                    board.Port.Read(buffer, 0, bytes);
                    foreach (byte b in buffer)
                        Console.Write("{0} ", b);
                    Console.WriteLine();*/

                    s = board.Port.ReadExisting();
                    Console.WriteLine(s);
                    if (s.Contains("CWI"))
                        break;
                }
                catch (TimeoutException)
                {
                }
                Thread.Sleep(100);
            }

            //board.Port.DiscardInBuffer()
            string t = String.Empty;
            while (true)
            {
                board.SendReadyPacket();
                try
                {
                    int bytes = board.Port.BytesToRead;
                    if (bytes > 0)
                    {
                        byte[] buffer = new byte[bytes];
                        board.Port.Read(buffer, 0, bytes);
                        foreach (byte b in buffer)
                        {
                            Console.Write("{0} ", b);
                            if (b == 0x41)
                            {
                                Console.WriteLine("Ack!");
                                break;
                            }

                        }
                        Console.WriteLine();
                    }
                }
                catch (TimeoutException)
                {
                }
                Thread.Sleep(100);
            }
        }

    }
}
