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
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Product ID: {0}, App Ver: {1}, Boot Ver: {2}", this.ProductID, this.AppFirmwareVersion, this.BootFirmwareVersion);
                return sb.ToString();
            }
        }

        public ASWCInfo ASWCInformation { get; set; }
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

            this.PacketSize = 44;
            this.IntroPacket = this.PrepPacket(new byte[] { 0x01, 0xF0, 0x10, 0x03, 0xA0, 0x01, 0x0F, 0x58, 0x04 });
            this.ReadyPacket = this.PrepPacket(new byte[] { 0x01, 0xF0, 0x20, 0x00, 0xEB, 0x04 });
            //0x55, 0xB0, 0x09, 0x01, 0xF0, 0xA0, 0x03, 0x10, 0x01, 0x00, 0x57, 0x04
            this.ASWCRequestPacket = this.PrepASWCRequestPacket(new byte[] { 0x55, 0xB0, 0x09, 0x01, 0xF0, 0xA0, 0x03, 0x10, 0x01, 0x00, 0x57, 0x04 });

            this.Initialize();
        }

        protected virtual void Initialize()
        {
            this.Type = BoardType.HIDNoChecksum;
            this.OnIntro += ParseIntroPacket;
            Log.Write("Initialized HID-2 board.", LogMode.Verbose);
        }

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



        //Atomic packet operations
        public void SendIntroPacket()
        {
            this.Write(new IntroReport(this));
        }
        public void SendReadyPacket()
        {
            this.Write(new ReadyReport(this));
        }
        public virtual void SendASWCRequestPacket()
        {
            Log.Write("Sending ASWC Info request packet.");
            this.Write(new RawOutputReport(this, this.ASWCRequestPacket));
        }

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
        /// <returns>True of intro packet, else false</returns>
        protected virtual void ParseIntroPacket(object sender, PacketEventArgs args)
        {
            byte[] packet = args.Packet;
            String content = String.Empty;

            //Parse packet into characters            
            foreach (byte b in packet)
            {
                content += Convert.ToChar(b);
            }

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
        //Event related stuff
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
            /*return packet[1] == 0x01
                && packet[2] == 0x0F
                && packet[3] == 0xA0;*/
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
        /// It will forward the packet to the appropriate event handler based on identification
        /// </summary>
        /// <param name="oInRep">The input report containing the packet</param>
        protected override void HandleDataReceived(InputReport InRep)
        {
            base.HandleDataReceived(InRep);

            byte[] packet = InRep.Buffer;
            /*Console.Write("Incoming Report: ");
            Report.PrintReport(InRep);

            if (this.ProductID.Equals(String.Empty))
            {
                if (this.ProcessIntroPacket(packet))
                    this.OnIntroReceived(new PacketEventArgs(packet));
            }*/ 
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

        public virtual void RegisterASWCData(byte[] raw)
        {
            this.ASWCInformation = new ASWCInfo(raw);
        }

        public virtual void SendASWCMappingRequest(ASWCInfo info, IList<SectionChanged> list)
        {
            List<byte> packet = new List<byte>(info.GetRawPacket(list));
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

        public virtual byte CalculateChecksum(byte[] packet)
        {
            byte checksum = new byte();
            foreach (byte b in packet)
                checksum ^= b;
            return checksum;
        }
        
        #region Events
        public event IntroEventHandler OnIntro;
        public event AckEventHandler OnAck;
        public event FinalEventHandler OnFinal;
        public event ASWCInfoHandler OnASWCInfo;
        public event ASWCConfirmHandler OnASWCConfirm;
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