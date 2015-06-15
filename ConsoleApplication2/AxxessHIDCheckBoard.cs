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

        protected override bool ParseIntroPacket(byte[] packet)
        {
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
                return true;
            }
            else { return false; }
        }

        protected override bool ProcessIntroPacket(byte[] packet)
        {
            return ParseIntroPacket(packet);
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
            byte checksum = new byte();
            foreach (byte b in newPacket)
                checksum ^= b;
            newPacket[64] = checksum;

            return newPacket;
        }        
    }
}
