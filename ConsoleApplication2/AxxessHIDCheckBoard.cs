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
        public AxxessHIDCheckBoard()
            : base()
        {
            this.Type = BoardType.HIDChecksum;
            this.PacketSize = 44;
            this.IntroPacket = this.PrepPacket(new byte[] { 0x01, 0xF0, 0x10, 0x03, 0xA0, 0x01, 0x0F, 0x58, 0x04 });
            this.ReadyPacket = this.PrepPacket(new byte[] { 0x01, 0xF0, 0x20, 0x00, 0xEB, 0x04 });
        }

        /// <summary>
        /// Method to extract board versioning info from intro response packets
        /// </summary>
        /// <param name="packet">The received packet</param>
        /// <returns>True of intro packet, else false</returns>
        protected bool ParseIntroPacket(byte[] packet)
        {
            //Parse packet into characters
            string content = String.Empty;
            foreach (byte b in packet)
            {
                content += Convert.ToChar(b);
            }
            
            if (content.Substring(10,3).Equals("CWI"))
            {
                this.ProductID = Convert.ToInt32(content.Substring(13, 7));
                try
                {
                    this.AppFirmwareVersion = Convert.ToInt32(content.Substring(29, 3));
                }
                catch (FormatException e)
                {
                    this.AppFirmwareVersion = 0;
                }
                return true;
            }
            else { return false; }
        }

        protected override bool ProcessIntroPacket(byte[] packet)
        {
            return this.ParseIntroPacket(packet);
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

        /// <summary>
        /// Takes a packet and prepares it for transmission to the board.  Adds leading bytes, padding and checksum.
        /// </summary>
        /// <param name="packet">The packet to prepare for sending.</param>
        /// <returns></returns>
        public override byte[] PrepPacket(byte[] packet)
        {
            byte[] newPacket = new byte[65];
            byte[] content = base.PrepPacket(packet);

            //Add leading header
            newPacket[1] = 0x55;
            newPacket[2] = 0xB0;

            //Content length byte added
            newPacket[3] = Convert.ToByte(content.Length);

            //Add content bytes
            for (int i=0; i<content.Length; i++)
                newPacket[i + 4] = content[i];

            byte checksum = new byte();
            foreach (byte b in newPacket)
                checksum ^= b;
            newPacket[64] = checksum;

            return newPacket;
        }        
    }
}
