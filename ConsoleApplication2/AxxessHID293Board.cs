using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace Metra.Axxess
{
   /// <summary>
    /// Class based implementation of the HID w/ Checksum board
    /// </summary>
    public class AxxessHID293Board : AxxessHIDBoard
    {
        public AxxessHID293Board() : base() { }
        protected override void Initialize()
        {
            this.Type = BoardType.HIDThree;
            this.PacketSize = 62;

            this.IntroPacket = this.PrepPacketWithoutCheck(new byte[] { 0x01, 0xF0, 0x10, 0x03, 0xA0, 0x01, 0x0F, 0x58, 0x04 });
            this.ReadyPacket = this.PrepPacketWithoutCheck(new byte[] { 0x01, 0xF0, 0x20, 0x3F, 0xEB, 0xC5 });
            this.ASWCRequestPacket = this.PrepPacketWithoutCheck(new byte[] { 0x55, 0xB0, 0x09, 0x01, 0xF0, 0xA0, 0x03, 0x10, 0x01, 0x00, 0x57, 0x04 });
            this.ASWCRequestPacket = this.PrepPacket(new byte[] { 0xF0, 0xA0, 0x03, 0x10, 0x01, 0x00, 0x57, 0x04 });

            this.OnIntro += ParseIntroPacket;
        }

        //01 0F 20 00 CC 04
        public override bool IsAck(byte[] packet) 
        {
            /*if (packet[1] == 0x01 && packet[2] == 0x0F && packet[3] == 0x20 && 
                packet[4] == 0x00 && packet[5] == 0xCC && packet[6] == 0x04)*/
            if (packet.Length > 0)
            {
                //Console.WriteLine("Ack!");
                return true;
            }
            return false;
        
        }
        //No final packet on this board type
        public override bool IsFinal(byte[] packet)
        {
            return false;
        }

        protected override void HandleDataReceived(InputReport InRep)
        {
            /*byte[] packet = InRep.Buffer;
            foreach (byte b in packet)
                Console.Write("{0} ", b);
            Console.WriteLine();*/

            /*if (this.ProductID.Equals(String.Empty))
            {
                if (this.ProcessIntroPacket(packet))
                    this.OnIntroReceived(new PacketEventArgs(packet));
            }*/
            base.HandleDataReceived(InRep);
        }

        /*protected override bool ProcessIntroPacket(byte[] packet)
        {
            return (this.ParseIntroPacket(packet));
        }*/
        protected override void ParseIntroPacket(object sender, PacketEventArgs args)
        {
            byte[] packet = args.Packet;
            //Parse packet into characters
            String content = String.Empty;
            foreach (byte b in packet)
            {
                content += Convert.ToChar(b);
            }

            //Console.WriteLine(packet);

            if (content.Substring(7, 3).Equals("CWI"))
            {
                this.ProductID = content.Substring(7, 9);
                this.AppFirmwareVersion = content.Substring(26, 2);
                this.OnIntro -= ParseIntroPacket;
            }
        }

        /// <summary>
        /// Takes a packet and prepares it for transmission to the board.  Adds leading bytes, padding and optional checksum.
        /// </summary>
        /// <param name="packet">The packet to prepare for sending.</param>
        /// <returns></returns>
        public byte[] PrepPacketWithoutCheck(byte[] packet, bool encapsulate = false)
        {
            byte[] newPacket = new byte[65];

            //Add content bytes
            for (int i = 0; i < packet.Length; i++)
            {
                if (encapsulate)
                    newPacket[i + 2] = packet[i];
                else
                    newPacket[i + 1] = packet[i];
            }

            return newPacket;
        }
        public override byte[] PrepPacket(byte[] packet)
        {
            byte[] newPacket = PrepPacketWithoutCheck(packet, true);

            //Encapsulate
            newPacket[1] = 0x01;
            byte by = this.CalculateChecksum(newPacket);
            newPacket[64] = (byte)(by & 0xFF);
            //Console.WriteLine("Post checksum: {0}", newPacket[64]);

            /*Console.Write("Sending: ");
            foreach (byte b in newPacket)
                Console.Write("{0} ", b);
            Console.WriteLine();*/

            return newPacket;
        }

        public override byte CalculateChecksum(byte[] packet)
        {
            //Sum up all bytes except the lead
            int checksum = 0;

            for (int i = 2; i < packet.Length-1; i++)
            {
                //checksum = Util.AddBytes(checksum, packet[i]);
                checksum += packet[i];
            }

            //Console.WriteLine("Raw checksum: {0}", checksum);
            return (byte)checksum;
        } 
        
    }
}
