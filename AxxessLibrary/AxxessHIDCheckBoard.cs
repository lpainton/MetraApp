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
    public class AxxessHIDCheckBoard : AxxessHIDBoard
    {
        public AxxessHIDCheckBoard() : base() { }
        protected override void Initialize()
        {
            this.Type = BoardType.HIDChecksum;
            Log.Write("Initialized HID-1 board.", LogMode.Verbose);
            this.OnIntro += ParseIntroPacket;
        }

        public override bool IsAck(byte[] packet) 
        {
            return ((packet[4] == 0x41)
                || (packet[5] == 0x41)
                || (packet[6] == 0x41));
        
        }
        public override bool IsFinal(byte[] packet)
        {
            return (packet[4] == 0x38);
        }
        public override bool IsASWCRead(byte[] packet)
        {
            return packet[4] == 0x01
                && packet[5] == 0x0F
                && packet[6] == 0xA0;
        }
        public override bool IsASWCConfirm(byte[] packet)
        {
            return packet[4] == 0x01
                && packet[5] == 0x0F
                && packet[6] == 0xA1
                && packet[7] == 0x01;
        }

        protected override void ParseIntroPacket(object sender, PacketEventArgs args)
        {
            byte[] packet = args.Packet;

            //Parse packet into characters
            String content = String.Empty;
            foreach (byte b in packet)
            {
                content += Convert.ToChar(b);
            }

            if (content.Substring(10, 3).Equals("CWI"))
            {
                this.ProductID = content.Substring(10, 9);
                this.AppFirmwareVersion = content.Substring(29, 3);
                Log.Write("Intro packet parsed " + this.Info, LogMode.Verbose);
                this.OnIntro -= ParseIntroPacket;
            }
        }

        /// <summary>
        /// Takes a packet and prepares it for transmission to the board.  Adds leading bytes, padding and checksum.
        /// </summary>
        /// <param name="packet">The packet to prepare for sending.</param>
        /// <returns></returns>
        public override byte[] PrepPacket(byte[] packet)
        {
            byte[] newPacket = new byte[65];
            byte[] content = packet;

            //Add leading header
            newPacket[1] = 0x55;
            newPacket[2] = 0xB0;

            //Content length byte added
            newPacket[3] = Convert.ToByte(content.Length);

            //Add content bytes
            for (int i = 0; i < content.Length; i++)
                newPacket[i + 4] = content[i];

            //Append checksum

            newPacket[64] = CalculateChecksum(newPacket);

            return newPacket;
        }
        protected override byte[] PrepASWCRequestPacket(byte[] packet)
        {
            //0x55, 0xB0, 0x09, 0x01, 0xF0, 0xA0, 0x03, 0x10, 0x01, 0x00, 0x57, 0x04
            //byte[] newPacket = new byte[59];
            byte[] newPacket = new byte[64];

            //Add content bytes
            for (int i = 0; i < packet.Length; i++)
                newPacket[i + 1] = packet[i];

            return newPacket;
        }

        public override byte CalculateChecksum(byte[] packet)
        {
            byte checksum = new byte();
            foreach (byte b in packet)
                checksum ^= b;
            return checksum;
        }

        protected override void ProcessASWCPacket(byte[] packet)
        {
            byte[] raw = new byte[59];
            Array.Copy(packet, 4, raw, 0, 59);
            this.RegisterASWCData(raw);
        }

        public override void SendASWCRequestPacket()
        {
            List<byte> packet = new List<byte>(this.ASWCRequestPacket);
            //packet.InsertRange(0, new byte[4] { 0x00, 0x55, 0xB0, 0x3B });
            //packet.Add(0x04);
            byte csum = CalculateChecksum(packet.ToArray());
            packet.Add(csum);
            this.Write(new RawOutputReport(this, packet.ToArray()));
        }

        public override void SendASWCMappingRequest(ASWCInfo info, IList<SectionChanged> list)
        {
            List<byte> packet = new List<byte>(info.GetRawPacket(list));
            packet.InsertRange(0, new byte[4] { 0x00, 0x55, 0xB0, 0x3B });
            packet.Add(0x04);
            byte csum = CalculateChecksum(packet.ToArray());
            packet.Add(csum);
            this.Write(new RawOutputReport(this, packet.ToArray()));
        }
    }
}
