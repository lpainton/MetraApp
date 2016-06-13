using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Management;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;

namespace Metra.Axxess
{
    class Program
    {
        public static void Main()
        {
            Queue<String> log = new Queue<string>();

            while (true)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Axxess Debugging Menu...");
                sb.AppendLine("T) Board Type Test");
                sb.AppendLine("I) Intro Test");
                sb.AppendLine("A) ASWC Request");
                sb.AppendLine("3) ASWC Byte-3 Testing");
                sb.AppendLine("F) Firmware JSON Testing");
                sb.AppendLine("Q) Quit");
                sb.AppendLine("Press key corresponding to option:");
                Console.Write(sb.ToString());

                ConsoleKeyInfo cki = Console.ReadKey();
                Console.WriteLine();
                switch (cki.Key)
                {
                    case ConsoleKey.T:
                        TypeTest();
                        break;

                    case ConsoleKey.I:
                        IntroTest();
                        break;

                    case ConsoleKey.A:
                        ASWCRequestTest();
                        break;

                    case ConsoleKey.D3:
                        ASWCByte3Test();
                        break;

                    case ConsoleKey.F:
                        FirmwareJSONTest();
                        break;

                    default:
                        return;
                }
            }
        }

        class TestToken
        {
            public string ua { get; set; }
            public string url { get; set; }
        }

        private static void FirmwareJSONTest()
        {
            string url = "http://axxessupdater.com/admin/secure/data-request.php?id=CWI257291&v=327&d=iPhone";

            WebClient www = new WebClient();
            string res = www.DownloadString(url);
            Console.WriteLine(res);

            TestToken tok = (TestToken)JsonConvert.DeserializeObject<TestToken>(res);
            Console.WriteLine(tok.ua);
            Console.WriteLine(tok.url);
        }

        private static void ASWCByte3Test()
        {   
            Console.WriteLine("------------------------------");
            Console.WriteLine("Testing ASWC Section Changed functionality:");
            ASWCInfo test = new ASWCInfo();

            List<SectionChanged> sections = new List<SectionChanged>(
                new SectionChanged[] { SectionChanged.Car, SectionChanged.SpeedControl });
            byte eval = (int)SectionChanged.Car | (int)SectionChanged.SpeedControl;
            byte[] packet = test.GetRawPacket(sections);
            Debug.Assert(packet[3] == eval);
            Console.WriteLine("Test #1: Expected {0} and got {1}.", eval, packet[3]);

            sections.Add(SectionChanged.PressHold);
            eval = (byte)(eval | (int)SectionChanged.PressHold);
            packet = test.GetRawPacket(sections);
            Debug.Assert(packet[3] == eval);
            Console.WriteLine("Test #2: Expected {0} and got {1}.", eval, packet[3]);

            sections.Remove(SectionChanged.Car);
            eval = eval = (byte)(eval ^ (int)SectionChanged.Car);
            packet = test.GetRawPacket(sections);
            Debug.Assert(packet[3] == eval);
            Console.WriteLine("Test #3: Expected {0} and got {1}.", eval, packet[3]);

            Console.WriteLine("------------------------------");
        }

        private static void ASWCRequestTest()
        {
            IAxxessBoard dev = GetBoard();
            ASWCInfo info = null;
            ManualResetEventSlim mre = new ManualResetEventSlim();
            ASWCInfoHandler d = delegate(object s, EventArgs a)
            {
                info = ((ASWCEventArgs)a).Info;
                mre.Set();
            };
            dev.AddASWCInfoEvent(d);
            Console.WriteLine("Beginning ASWC request test...");

            while(!mre.IsSet)
            {
                Console.WriteLine("Sending ASWC Request Packet.");
                dev.SendASWCRequestPacket();
                mre.Wait(100);
            }

            Console.WriteLine("Displaying parsed ASWC info:");
            Console.WriteLine(info.ToString());
        }

        private static IAxxessBoard GetBoard()
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine("Connect a board to the system:");
            IAxxessBoard dev = null;
            while (dev == null)
            {
                dev = AxxessConnector.InitiateConnection();
                Thread.Sleep(10);
            }
            return dev;
        }

        private static void TypeTest()
        {
            IAxxessBoard dev = GetBoard();
            Console.WriteLine("Board type is " + dev.Type.ToString());
            Console.WriteLine("------------------------------");
        }

        private static void IntroTest()
        {
            IAxxessBoard dev = GetBoard();
            byte[] packet = null;
            IntroEventHandler d = delegate(object s, PacketEventArgs a)
            {
                packet = a.Packet;
            };


            dev.AddIntroEvent(d);

            Console.WriteLine("Beginning Intro Packet Test...");
            int counter = 0;
            while(packet == null)
            {
                Console.WriteLine("Sending Intro Packet.");
                counter++;
                dev.SendIntroPacket();
                Thread.Sleep(50);
            }
            dev.RemoveIntroEvent(d);
            Console.WriteLine("Characterizing Reply Packet...");
            Report.CharacterizeBuffer(packet);
            Console.WriteLine("Displaying Parsed Board Info...");
            Console.WriteLine(dev.Info);

            Console.WriteLine("------------------------------");
        }
    }
}
