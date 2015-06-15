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
    public class AxxessHID3Board : AxxessHIDBoard
    {
        public AxxessHID3Board() : base() { }
        protected override void Initialize()
        {
            this.Type = BoardType.HIDThree;
            this.PacketSize = 62;

            this.IntroPacket = this.PrepPacketWithoutCheck(new byte[] { 0xF0, 0x10, 0x03, 0xA0, 0x01, 0x0F, 0x58, 0x04 });
            this.ReadyPacket = this.PrepPacketWithoutCheck(new byte[] { 0xF0, 0x20, 0x3F, 0xEB, 0xC5 });
        }

        //01 0F 20 00 CC 04
        public override bool IsAck(byte[] packet) 
        {
            return ((packet[4] == 0x00)
                && (packet[5] == 0xCC)
                && (packet[6] == 0x04));
        
        }
        //No final packet on this board type
        public override bool IsFinal(byte[] packet)
        {
            return false;
        }

        /// <summary>
        /// Takes a packet and prepares it for transmission to the board.  Adds leading bytes, padding and checksum.
        /// </summary>
        /// <param name="packet">The packet to prepare for sending.</param>
        /// <returns></returns>
        public byte[] PrepPacketWithoutCheck(byte[] packet)
        {
            byte[] newPacket = new byte[65];
            byte[] content = packet;

            //Add leading byte
            newPacket[1] = 0x01;

            //Add content bytes
            for (int i = 0; i < content.Length; i++)
                newPacket[i + 2] = content[i];

            return newPacket;
        }
        public override byte[] PrepPacket(byte[] packet)
        {
            byte[] newPacket = PrepPacketWithoutCheck(packet);

            //Append checksum
            newPacket[64] = this.CalcChecksum(newPacket);

            return newPacket;
        }

        private byte CalcChecksum(byte[] packet)
        {
            //Sum up all bytes except the lead
            byte checksum = 0x00;

            for (int i = 2; i < packet.Length; i++)
            {
                checksum = Util.AddBytes(checksum, packet[i]);
            }

            return checksum;
        } 
        
    }
}
