using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    /// <summary>
    /// Basically just an exception of a special sub type.
    /// </summary>
    public class AxxessDeviceException : Exception
    {
    }
    
    /// <summary>
    /// An enum representing the current status of the board as it regards updates and remapping.
    /// Idle: Board is in boot mode and is idling
    /// Standby: Board is updating firmware and awaiting next transmission
    /// </summary>
    public enum BoardStatus
    {
        Idle,
        Standby
    };

    /// <summary>
    /// An enum containing all possible board types.
    /// </summary>
    public enum BoardType
    {
        HIDChecksum,
        HIDNoChecksum,
        HIDThree,
        MicrochipCDC,
        FTDI
    };

    /// <summary>
    /// Only includes possible response packets from the board.  
    /// Not currently being used.
    /// </summary>
    public enum PacketType
    {
        Intro,
        Ack,
        Final,
        ASWCInfo,
        ASWCConfirm
    };

    /// <summary>
    /// Defines an event to take place on receiving an intro reply from a connected device.
    /// </summary>
    public delegate void IntroEventHandler(object sender, PacketEventArgs e);
    /// <summary>
    /// Defines an event to take place on receiving a firmware ack.
    /// </summary>
    public delegate void AckEventHandler(object sender, EventArgs e);
    /// <summary>
    /// Defines an event to take place on receiving a firmware finalized packet.
    /// </summary>
    public delegate void FinalEventHandler(object sender, EventArgs e);
    /// <summary>
    /// Defines an event to take place on receiving an ASWCInfo packet.
    /// </summary>
    public delegate void ASWCInfoHandler(object sender, EventArgs e);
    /// <summary>
    /// Defines an event to take place on receiving an ASWC mapping confirmation.
    /// </summary>
    public delegate void ASWCConfirmHandler(object sender, EventArgs e);
    /// <summary>
    /// Defines an event to take place on receiving any packet.  
    /// Intended to provide custom functionality.
    /// </summary>
    public delegate void PacketHandler(object sender, PacketEventArgs e);

    /// <summary>
    /// Stores and provides access to the contents of the packet which triggered the event.
    /// </summary>
    public class PacketEventArgs : EventArgs
    {
        public byte[] Packet { get; private set; }

        public PacketEventArgs(byte[] packet) : base()
        {
            this.Packet = packet;
        }
    }

    /// <summary>
    /// Stores and provides access to the ASWCInfo obejct which triggered the event.
    /// </summary>
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

        /// <summary>
        /// Returns the type of the board in question.
        /// </summary>
        BoardType Type { get; }

        /// <summary>
        /// Returns the board's relevant packet size.
        /// Actual transmitted packets may be larger than this, 
        /// but this is how many bytes the board actually reads.
        /// </summary>
        int PacketSize { get; }

        /// <summary>
        /// Method to prepare a packet for transmission.
        /// </summary>
        /// <param name="packet">The raw packet.</param>
        /// <returns>The prepared packet.</returns>
        byte[] PrepPacket(byte[] packet);

        /// <summary>
        /// Returns a fully prepared intro packet, ready to be transmitted.
        /// </summary>
        byte[] IntroPacket { get; }

        /// <summary>
        /// Returns a fully prepared ready packet.  Packet is used to tell
        /// the board to get ready to receive firmware.
        /// </summary>
        byte[] ReadyPacket { get; }

        /// <summary>
        /// Returns a fully prepared ASWCInfo request packet.
        /// </summary>
        byte[] ASWCRequestPacket { get; }

        /// <summary>
        /// Returns a checksum calculated from the provided packet.
        /// </summary>
        /// <param name="packet">The relevant part of the packet.</param>
        /// <returns>A checksum in byte form.</returns>
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
        void SendASWCMappingPacket(ASWCInfo map, IList<SectionChanged> changes);

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

        /// <summary>
        /// General event fired whenever a packet is received.
        /// </summary>
        void AddPacketEvent(PacketHandler handler);
        void RemovePacketEvent(PacketHandler handler);

        void Dispose();
    }
}
