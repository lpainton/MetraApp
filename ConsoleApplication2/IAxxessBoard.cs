using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    public class AxxessDeviceException : Exception
    {
    }
    
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
        HIDThree,
        MicrochipCDC,
        FTDI
    };

    /// <summary>
    /// Only includes possible response packets from the board.  Not being used.
    /// </summary>
    public enum PacketType
    {
        Intro,
        Ack,
        Final,
        ASWCInfo,
        ASWCConfirm
    };

    public delegate void IntroEventHandler(object sender, EventArgs e);
    public delegate void AckEventHandler(object sender, EventArgs e);
    public delegate void FinalEventHandler(object sender, EventArgs e);
    public delegate void ASWCInfoHandler(object sender, EventArgs e);
    public delegate void ASWCConfirmHandler(object sender, EventArgs e);

    public class PacketEventArgs : EventArgs
    {
        public byte[] Packet { get; private set; }

        public PacketEventArgs(byte[] packet) : base()
        {
            this.Packet = packet;
        }
    }

    public class ASWCEventArgs : EventArgs
    {
        public ASWCInfo Info { get; private set; }

        public ASWCEventArgs(ASWCInfo info) : base()
        {
            this.Info = info;
        }
    }

    public interface IAxxessBoard
    {
        /// <summary>
        /// Stores the numeric product ID for the device
        /// </summary>
        string ProductID { get; }

        /// <summary>
        /// Stores the application firmware version number
        /// </summary>
        string AppFirmwareVersion { get; }

        /// <summary>
        /// Stores the boot firmware version number
        /// </summary>
        string BootFirmwareVersion { get; }

        /// <summary>
        /// Gets summary info on the device
        /// </summary>
        string Info { get; }

        /// <summary>
        /// Gets the ASWC info for the board
        /// </summary>
        ASWCInfo ASWCInformation { get; }

        BoardType Type { get; }

        int PacketSize { get; }

        byte[] PrepPacket(byte[] packet);
        byte[] IntroPacket { get; }
        byte[] ReadyPacket { get; }
        byte[] ASWCRequestPacket { get; }
        byte CalculateChecksum(byte[] packet);

        /// <summary>
        /// Sends an intro packet to the board.
        /// </summary>
        void SendIntroPacket();

        /// <summary>
        /// Sends a ready packet to the board which tells it to prepare for a firmware update.
        /// </summary>
        void SendReadyPacket();

        /// <summary>
        /// Sends an ASWC request packet to get information on button mapping.
        /// </summary>
        void SendASWCRequestPacket();

        /// <summary>
        /// Sends an ASWC write packet to attempt button remap.
        /// </summary>
        void SendASWCMappingPacket(ASWCInfo map);

        /// <summary>
        /// Sends the provided packet.
        /// </summary>
        /// <param name="packet">A raw byte array packet to send.</param>
        void SendPacket(byte[] packet);

        /// <summary>
        /// Sends the provided packet without processing.
        /// </summary>
        /// <param name="packet">The packet to send in raw form.</param>
        void SendRawPacket(byte[] packet);

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

        /// <summary>
        /// Fired when the device is removed
        /// </summary>
        void AddRemovedEvent(EventHandler handler);
        void RemoveRemovedEvent(EventHandler handler);

        /// <summary>
        /// Fired when an ASWC info packet is receieved.
        /// </summary>
        void AddASWCInfoEvent(ASWCInfoHandler handler);
        void RemoveASWCInfoEvent(ASWCInfoHandler handler);

        /// <summary>
        /// Fired when an ASWC confirmation is receieved.
        /// </summary>
        void AddASWCConfimEvent(ASWCConfirmHandler handler);
        void RemoveASWCConfimEvent(ASWCConfirmHandler handler);

        void Dispose();
    }
}
