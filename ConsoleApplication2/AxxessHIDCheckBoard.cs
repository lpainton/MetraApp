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
            byte[] newPacket = base.PrepPacket(packet);

            //Append checksum
            byte checksum = new byte();
            foreach (byte b in newPacket)
                checksum ^= b;
            newPacket[64] = checksum;

            return newPacket;
        }        
    }
}
