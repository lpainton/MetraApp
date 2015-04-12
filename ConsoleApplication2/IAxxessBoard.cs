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

    public delegate void IntroEventHandler(object sender, EventArgs e);
    public delegate void AckEventHandler(object sender, EventArgs e);
    public delegate void FinalEventHandler(object sender, EventArgs e);

    class PacketEventArgs : EventArgs
    {
        byte[] Packet { get; private set; }

        public PacketEventArgs(byte[] packet) : base()
        {
            this.Packet = packet;
        }
    }

    interface IAxxessBoard
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

        int PacketSize { get; }

        byte[] PrepPacket(byte[] packet);
        byte[] IntroPacket { get; }
        byte[] ReadyPacket { get; }

        /// <summary>
        /// Sends an intro packet to the board.
        /// </summary>
        void SendIntroPacket();

        /// <summary>
        /// Sends a ready packet to the board which tells it to prepare for a firmware update.
        /// </summary>
        void SendReadyPacket();

        /// <summary>
        /// Sends the provided packet.
        /// </summary>
        /// <param name="packet">A raw byte array packet to send.</param>
        void SendPacket(byte[] packet);

        /// <summary>
        /// Fired when an intro packet is received from the board.
        /// </summary>
        void AddIntroEvent(IntroEventHandler handler);
        void RemoveIntroEvent(IntroEventHandler handler);

        /// <summary>
        /// Fired when the board sends and ack.
        /// </summary>
        void AddAckEvent(AckEventHandler handler);
        void RemoveAckEvent(AckEventHandler handler);

        /// <summary>
        /// Fired when the board is finished with the update process.
        /// </summary>
        void AddFinalEvent(FinalEventHandler handler);
        void RemoveFinalEvent(FinalEventHandler handler);
    }
}
