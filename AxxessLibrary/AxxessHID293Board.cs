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
    /// Class represents the Axxess HID w/ Checksum board.
    /// </summary>
    /// <remarks>
    /// The HID 293 (HID-3) board is different from other HIDs in both its protocols
    /// and in its requirement for a DC-12 volt power source.
    /// </remarks>
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

        /// This is notably different from other boards in that any response data 
        /// is considered acknowledgement of packet receipt.
        public override bool IsAck(byte[] packet) 
        {
            if (packet.Length > 0)
            {
                return true;
            }
            return false;
        
        }
        //No final packet on this board type
        public override bool IsFinal(byte[] packet)
        {
            return false;
        }

        //Not necessary
        protected override void HandleDataReceived(InputReport InRep)
        {
            base.HandleDataReceived(InRep);
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

            if (content.Substring(7, 3).Equals("CWI"))
            {
                this.ProductID = content.Substring(7, 9);
                this.AppFirmwareVersion = content.Substring(26, 2);
                this.OnIntro -= ParseIntroPacket;
            }
        }

        /// <summary>
        /// Takes a packet and prepares it for transmission to the board.  Adds leading bytes and padding as needed.
        /// </summary>
        /// <param name="packet">The packet to prepare for sending.</param>
        /// <param name="encapsulate">Optional param.  Only set to true if you intend to add a checksum on the end.</param>
        /// <returns>The prepared packet</returns>
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
        /// <summary>
        /// Prepares the packet with encapsulation and a checksum.
        /// </summary>
        /// <param name="packet">The raw packet to prepare</param>
        /// <returns>The prepared packet with encapsulation and checksum.</returns>
        public override byte[] PrepPacket(byte[] packet)
        {
            byte[] newPacket = PrepPacketWithoutCheck(packet, true);

            //Encapsulate
            newPacket[1] = 0x01;
            byte by = this.CalculateChecksum(newPacket);
            newPacket[64] = (byte)(by & 0xFF);

            return newPacket;
        }

        /// <summary>
        /// Calculates a checksum appropriate for this board type.
        /// Does this by summing all bytes except the leading byte since it is a windows artifact.
        /// </summary>
        /// <param name="packet">The packet of bytes being considered.</param>
        /// <returns>The checksum as a byte</returns>
        public override byte CalculateChecksum(byte[] packet)
        {
            int checksum = 0;

            //Loop sums up all bytes except the lead
            for (int i = 2; i < packet.Length-1; i++)
            {
                checksum += packet[i];
            }

            return (byte)checksum;
        } 
        
    }
}
