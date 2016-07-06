using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Metra.Axxess
{
    /// <summary>
    /// Class represents an Axxess HID-2 style board.
    /// </summary>
    /// <remarks>
    /// Note that this type of board is ideal for library example purposes, 
    /// since most other types were based on its protocols.
    /// </remarks>
    public class AxxessHIDBoard : HIDDevice, IAxxessBoard
    {
        //Board attributes
        public virtual int PacketSize { get; protected set; }

        public string ProductID { get; protected set; }
        public string AppFirmwareVersion 
        {
            get
            {
                return _appVer.ToString();
            }
            protected set
            {
                _appVer = new AxxessFirmwareVersion(value);
            }   
        }
        AxxessFirmwareVersion _appVer;
        public string BootFirmwareVersion 
        {
            get
            {
                return _bootVer.ToString();
            }
            protected set
            {
                _bootVer = new AxxessFirmwareVersion(value);
            } 
        }
        AxxessFirmwareVersion _bootVer;
        public string Info 
        {
            get
            {
                return String.Format("Product ID: {0}, App Ver: {1}, Boot Ver: {2}", this.ProductID, this.AppFirmwareVersion, this.BootFirmwareVersion);
            }
        }

        /// <summary>
        /// Stores ASWC information attached to this board.
        /// </summary>
        public ASWCInfo ASWCInformation { get; set; }

        /// <summary>
        /// The current status of the board (idle or standby).  Only used for updating firmware.
        /// </summary>
        private BoardStatus Status { get; set; }        

        public byte[] IntroPacket { get; protected set; }
        public byte[] ReadyPacket { get; protected set; }
        public byte[] ASWCRequestPacket { get; protected set; }

        public BoardType Type { get; protected set; }

        public AxxessHIDBoard()
            : base()
        {
            this.ProductID = String.Empty;
            this.AppFirmwareVersion = String.Empty;
            this.BootFirmwareVersion = String.Empty;
            this.ASWCInformation = null;
            this.Status = BoardStatus.Idle;

            //This packet size is the transmission width for significant bytes in this board type.
            //Ultimately all packets sent over USB HID are 65 bytes in size.  Essentially everything
            //outside of these 44 bytes is overhead.
            this.PacketSize = 44;

            //These packets are hard coded since the protocols are.  Ideally an updated version of this library
            //would allow the definition of protocols using external JSON or XML configuration files.
            this.IntroPacket = this.PrepPacket(new byte[] { 0x01, 0xF0, 0x10, 0x03, 0xA0, 0x01, 0x0F, 0x58, 0x04 });
            this.ReadyPacket = this.PrepPacket(new byte[] { 0x01, 0xF0, 0x20, 0x00, 0xEB, 0x04 });
            this.ASWCRequestPacket = this.PrepASWCRequestPacket(new byte[] { 0x55, 0xB0, 0x09, 0x01, 0xF0, 0xA0, 0x03, 0x10, 0x01, 0x00, 0x57, 0x04 });

            this.Initialize();
        }

        /// <summary>
        /// Modularizes some of the initialization steps since they're specific to HID-2 types.
        /// Inheriting boards override some of these.
        /// </summary>
        protected virtual void Initialize()
        {
            this.Type = BoardType.HIDNoChecksum;
            this.OnIntro += ParseIntroPacket;
            Log.Write("Initialized HID-2 board.", LogMode.Verbose);
        }

        /// <summary>
        /// Prepares a packet requesting ASWC information from the board for transmission.
        /// </summary>
        /// <param name="packet">The raw packet to prepare.</param>
        /// <returns>The prepped packet.</returns>
        /// <remarks>
        /// Microsoft's HID protocols require the addition of a leading zero byte to any transmission.
        /// So where the normal HID tramission window is 64 bytes, Microsoft's is 65.
        /// </remarks>
        protected virtual byte[] PrepASWCRequestPacket(byte[] packet)
        {
            //byte[] newPacket = new byte[59];
            byte[] newPacket = new byte[65];
            byte[] content = packet;

            //Add content bytes
            for (int i = 0; i < content.Length; i++)
                newPacket[i + 1] = content[i];

            return newPacket;
        }



        //--- Atomic packet operations ---
        //These discrete packet operations allow direct control over communication procotols with the board.
        public void SendIntroPacket()
        {
            this.Write(new IntroReport(this));
        }
        public void SendReadyPacket()
        {
            this.Write(new ReadyReport(this));
        }
        /// <summary>
        /// Sends an ASWC info request packet to the connected device.
        /// </summary>
        /// <remarks>
        /// The ASWC protocols being notably different from the normal HID-2 protocols
        /// require sending the packets without normal HID-2 protocol preparation.
        /// Hence the use of a RawOutputReport, which transmits the packet 'as is'.
        /// </remarks>
        public virtual void SendASWCRequestPacket()
        {
            Log.Write("Sending ASWC Info request packet.");
            this.Write(new RawOutputReport(this, this.ASWCRequestPacket));
        }
        /// <summary>
        /// Preps a packet for transmission.  Exactly the same as <see cref="PrepASWCRequestPacket(byte[])">PrepASWCRequestPacket</see>.
        /// </summary>
        public virtual byte[] PrepPacket(byte[] packet) 
        {
            byte[] newPacket = new byte[65];
            byte[] content = packet;

            //Add content bytes
            for (int i = 0; i < content.Length; i++)
                newPacket[i + 1] = content[i];

            return newPacket;
        }

        /// <summary>
        /// Method to extract board versioning info from intro response packets
        /// </summary>
        /// <param name="packet">The received packet</param>
        /// <returns>True if intro packet, else false</returns>
        /// <remarks>
        /// Note that if the board has corrupted firmware it may return a corrupt intro packet that
        /// registers as an intro packet but has an unparsable PID or Firmware Ver.
        /// </remarks>
        protected virtual void ParseIntroPacket(object sender, PacketEventArgs args)
        {
            byte[] packet = args.Packet;
            String content = String.Empty;

            //Parse packet into characters            
            foreach (byte b in packet)
            {
                content += Convert.ToChar(b);
            }

            //Intro packets contain ASCII encoded bytes which include
            //CWI followed by a PID and firmware version.  Only if all these bytes are in place
            //is the intro packet valid.
            if (content.Substring(7, 3).Equals("CWI"))
            {
                this.ProductID = content.Substring(7, 9);
                this.AppFirmwareVersion = content.Substring(26, 3);
                Log.Write("Intro packet parsed " + this.Info, LogMode.Verbose);
                this.OnIntro -= ParseIntroPacket;
            }
        }

        protected virtual bool IsIntro(byte[] packet)
        {
            return (packet[1] == 0x01 && packet[2] == 0x0F &&
                packet[3] == 0x10 && packet[4] == 0x16 &&
                packet[5] == 0x1A);
        }        

        //Event related condition checks
        public virtual bool IsAck(byte[] packet) 
        {
            return ((packet[1] == 0x41)
                || (packet[2] == 0x41)
                || (packet[3] == 0x41));
        }
        public virtual bool IsFinal(byte[] packet) 
        {
            return (packet[1] == 0x38);
        }
        public virtual bool IsASWCRead(byte[] packet)
        {
            return packet[4] == 0x01
                && packet[5] == 0x0F
                && packet[6] == 0xA0;
        }
        public virtual bool IsASWCConfirm(byte[] packet)
        {
            return packet[4] == 0x01
                && packet[5] == 0x0F
                && packet[6] == 0xA1
                && packet[7] == 0x01;
        }

        /// <summary>
        /// This method is called asynchronously when a packet is received.
        /// It will forward the packet chain of responsibility style
        /// to the appropriate event handler based on identification.
        /// </summary>
        /// <param name="oInRep">The input report containing the packet</param>
        protected override void HandleDataReceived(InputReport InRep)
        {
            base.HandleDataReceived(InRep);

            byte[] packet = InRep.Buffer;

            if (this.OnPacket != null) this.OnPacketReceived(new PacketEventArgs(packet));
            if (this.OnIntro != null) this.OnIntroReceived(new PacketEventArgs(packet));
            if (this.OnAck != null && this.IsAck(packet))
            {
                this.OnAckReceived(new PacketEventArgs(packet));
                return;
            }
            else if (this.OnFinal != null && this.IsFinal(packet))
            {
                this.OnFinalReceived(new PacketEventArgs(packet));
                return;
            }
            else if (this.OnASWCInfo != null && this.IsASWCRead(packet))
            {
                byte[] raw = new byte[61];
                for (int i = 4; i < packet.Length; i++)
                {
                    raw[i - 4] = packet[i];
                }
                this.OnASWCInfoReceieved(new ASWCEventArgs(new ASWCInfo(raw)));
            }
            else if (this.OnASWCConfirm != null && this.IsASWCConfirm(packet))
            {
                this.OnASWCConfirmReceieved(new PacketEventArgs(packet));
            }


        }

        protected virtual void ProcessASWCPacket(byte[] packet)
        {
            this.RegisterASWCData(packet);
        }

        /// <summary>
        /// Swaps out any existing ASWCInformation object held by this one with a new
        /// version from serial data.
        /// </summary>
        /// <param name="raw">The raw serial data</param>
        public virtual void RegisterASWCData(byte[] raw)
        {
            this.ASWCInformation = new ASWCInfo(raw);
        }

        /// <summary>
        /// Serializes an ASWCInfo object and then transmits it to the connected board.
        /// Builds a list of changed sections from the provided list.
        /// </summary>
        /// <param name="info">The info object</param>
        /// <param name="list"></param>
        /// <remarks>
        /// Notably the ASWC system requires a checksum, whereas technically this is listed as a 
        /// no-checksum board.  This is because the ASWC system was added after the board's creation
        /// and is standardized across all board types.        
        /// </remarks>
        public virtual void SendASWCMappingRequest(ASWCInfo info, IList<SectionChanged> list)
        {
            List<byte> packet = new List<byte>(info.Serialize(list));
            packet.InsertRange(0, new byte[4] { 0x00, 0x55, 0xB0, 0x3B });
            packet.Add(0x04);
            byte csum = CalculateChecksum(packet.ToArray());
            packet.Add(csum);
            this.Write(new RawOutputReport(this, packet.ToArray()));
        }

        public override InputReport CreateInputReport()
        {
            return new AxxessInputReport(this);
        }

        /// <summary>
        /// Calculates 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public virtual byte CalculateChecksum(byte[] packet)
        {
            byte checksum = new byte();
            foreach (byte b in packet)
                checksum ^= b;
            return checksum;
        }
        
        #region Events
        /// <summary>
        /// Fired on receiving intro reply packets from the device.
        /// </summary>
        public event IntroEventHandler OnIntro;
        /// <summary>
        /// Fired on receiving a firmware ack packet from the device.
        /// </summary>
        public event AckEventHandler OnAck;
        /// <summary>
        /// Fired on receiving a firmware finalization packet from the device.
        /// </summary>
        public event FinalEventHandler OnFinal;
        /// <summary>
        /// Fired on receiving an ASWCInfo packet from the device.
        /// </summary>
        public event ASWCInfoHandler OnASWCInfo;
        /// <summary>
        /// Fired on receiving an ASWC mapping confirmation from the device.
        /// </summary>
        public event ASWCConfirmHandler OnASWCConfirm;
        /// <summary>
        /// Fired on receiving any packet from the device.
        /// </summary>
        public event PacketHandler OnPacket;

        public virtual void OnIntroReceived(PacketEventArgs e) { OnIntro(this, e); }
        public virtual void OnAckReceived(EventArgs e) { OnAck(this, e); }
        public virtual void OnFinalReceived(EventArgs e) { OnFinal(this, e); }
        public virtual void OnASWCInfoReceieved(EventArgs e) { OnASWCInfo(this, e); }
        public virtual void OnASWCConfirmReceieved(EventArgs e) { OnASWCConfirm(this, e); }
        public virtual void OnPacketReceived(PacketEventArgs e) { OnPacket(this, e); }
        #endregion 

        #region Explicit IAxxessDevice Implementation
        string IAxxessBoard.ProductID { get { return this.ProductID; } }
        string IAxxessBoard.AppFirmwareVersion { get { return this.AppFirmwareVersion; } }
        string IAxxessBoard.BootFirmwareVersion { get { return this.BootFirmwareVersion; } }
        string IAxxessBoard.Info { get { return this.Info; } }
        ASWCInfo IAxxessBoard.ASWCInformation { get { return this.ASWCInformation; } }
        BoardType IAxxessBoard.Type { get { return this.Type; } }
        int IAxxessBoard.PacketSize { get { return this.PacketSize; } }

        byte[] IAxxessBoard.PrepPacket(byte[] packet) { return this.PrepPacket(packet); }
        byte[] IAxxessBoard.IntroPacket { get { return this.IntroPacket; } }
        byte[] IAxxessBoard.ReadyPacket { get { return this.ReadyPacket; } }

        void IAxxessBoard.SendIntroPacket() { this.SendIntroPacket(); }
        void IAxxessBoard.SendReadyPacket() { this.SendReadyPacket(); }
        void IAxxessBoard.SendASWCMappingPacket(ASWCInfo map, IList<SectionChanged> list) { this.SendASWCMappingRequest(map, list); }
        void IAxxessBoard.SendASWCRequestPacket() { this.SendASWCRequestPacket(); }
        void IAxxessBoard.SendPacket(byte[] packet) { this.Write(new GenericOutputReport(this, packet)); }
        void IAxxessBoard.SendRawPacket(byte[] packet) { this.Write(new RawOutputReport(this, packet)); }
        byte IAxxessBoard.CalculateChecksum(byte[] packet) { return this.CalculateChecksum(packet); }

        void IAxxessBoard.AddIntroEvent(IntroEventHandler handler) { this.OnIntro += handler; }
        void IAxxessBoard.RemoveIntroEvent(IntroEventHandler handler) { this.OnIntro -= handler; }
        void IAxxessBoard.AddAckEvent(AckEventHandler handler) { this.OnAck += handler; }
        void IAxxessBoard.RemoveAckEvent(AckEventHandler handler) { this.OnAck -= handler; }
        void IAxxessBoard.AddFinalEvent(FinalEventHandler handler) { this.OnFinal += handler; }
        void IAxxessBoard.RemoveFinalEvent(FinalEventHandler handler) { this.OnFinal -= handler; }
        void IAxxessBoard.AddRemovedEvent(EventHandler handler) { this.OnDeviceRemoved += handler; }
        void IAxxessBoard.RemoveRemovedEvent(EventHandler handler) { this.OnDeviceRemoved -= handler; }
        void IAxxessBoard.AddASWCInfoEvent(ASWCInfoHandler handler) { this.OnASWCInfo += handler; }
        void IAxxessBoard.RemoveASWCInfoEvent(ASWCInfoHandler handler) { this.OnASWCInfo -= handler; }
        void IAxxessBoard.AddASWCConfimEvent(ASWCConfirmHandler handler) { this.OnASWCConfirm += handler; }
        void IAxxessBoard.RemoveASWCConfimEvent(ASWCConfirmHandler handler) { this.OnASWCConfirm -= handler; }
        void IAxxessBoard.AddPacketEvent(PacketHandler handler) { this.OnPacket += handler; }
        void IAxxessBoard.RemovePacketEvent(PacketHandler handler) { this.OnPacket -= handler; }

        void IAxxessBoard.Dispose() { this.Dispose(); }

        #endregion
    }
}