using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    public enum PacketType
    {
        IntroPacket,
        ReadyPacket,
    };

    /// <summary>
    /// An enum representing the current status of the board as it regards updates and remapping.
    /// NoOp: Board is in a non-ready state
    /// Hailed: Initial intro packet has been sent
    /// Idling: Board is in pre-ready state but otherwise non-active
    /// Standby: Update cycle begun and awaiting board response
    /// Ready: Board is in ready-state and prepared to receive data packets
    /// Finalizing: Board has completed a data operation and is exiting ready-state.
    /// </summary>
    public enum BoardStatus
    {
        Idle,
        Standby
    };

    public enum BoardType
    {
        HIDChecksum,
        HIDNoChecksum,
    };

    public enum PacketType
    {
        Intro,
        Ack,
        Final
    };

    /*public class Packet
    {
        public byte[] Bytes { get; private set; }
        public PacketType Type { get; private set; }

        public Packet(byte[] packet, PacketType type)
        {
            this.Bytes = packet;
            this.Type = type;
        }
    }*/

    interface IAxxessDevice
    {
        /// <summary>
        /// Stores the numeric product ID for the device
        /// </summary>
        int ProductID { get; }

        /// <summary>
        /// Stores the application firmware version number
        /// </summary>
        int AppFirmwareVersion { get; }

        /// <summary>
        /// Stores the boot firmware version number
        /// </summary>
        int BootFirmwareVersion { get; }

        /// <summary>
        /// Forces idle status on the board.  Use when preparing for firmware installation.
        /// </summary>
        void StartForceIdle();

        byte[] PrepPacket(byte[] packet);
        byte[] IntroPacket { get; }
        byte[] ReadyPacket { get; }

        void SendIntroPacket();
    }
}
