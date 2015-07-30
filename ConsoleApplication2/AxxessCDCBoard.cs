using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;


namespace Metra.Axxess
{
    //Settings: baud=115200 parity=N data=8 stop=1
    public class AxxessCDCBoard : CDCDevice, IAxxessBoard
    {
        public bool TestMode { get; set; }

        //Board attributes
        public virtual int PacketSize { get; protected set; }

        public string ProductID { get; protected set; }
        public string AppFirmwareVersion { get; protected set; }
        public string BootFirmwareVersion { get; protected set; }
        public string Info 
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Product ID: " + this.ProductID);
                sb.AppendLine("App Ver: " + this.AppFirmwareVersion);
                sb.AppendLine("Boot Ver: " + this.BootFirmwareVersion);
                return sb.ToString();
            }
        }
        private BoardStatus Status { get; set; }        

        public byte[] IntroPacket { get; protected set; }
        public byte[] ReadyPacket { get; protected set; }
        public byte[] ASWCRequestPacket { get; protected set; }

        public BoardType Type { get; protected set; }

        //Settings: baud=115200 parity=N data=8 stop=1
        public AxxessCDCBoard() : this(false) { }

        public AxxessCDCBoard(bool testMode = false)
            : base(115200, Parity.None, 8, StopBits.One, Handshake.None, readTimeOut: 50, writeTimeOut: 500, testMode: testMode)
        {
            this.TestMode = testMode;

            this.Type = BoardType.MicrochipCDC;

            this.ProductID = String.Empty;
            this.AppFirmwareVersion = String.Empty;
            this.BootFirmwareVersion = String.Empty;
            this.Status = BoardStatus.Idle;

            this.PacketSize = 44;

            //Intro? 01 F0 10 03 A0 01 0F 58 04
            //Ready: 01 F0 20 00 EB 04
            this.IntroPacket = this.PrepPacket(new byte[] { 0x01, 0xF0, 0x10, 0x03, 0xA0, 0x01, 0x0F, 0x58, 0x04 });
            this.ReadyPacket = this.PrepPacket(new byte[] { 0x01, 0xF0, 0x20, 0x00, 0xEB, 0x04 });
            this.ASWCRequestPacket = this.PrepPacket(new byte[] { 0x01, 0xF0, 0xA0, 0x03, 0x10, 0x01, 0x00, 0x57, 0x04 });
        }       

        //Atomic packet operations
        public void SendIntroPacket()
        {
            this.Write(this.IntroPacket);
        }
        public void SendReadyPacket()
        {
            this.Write(this.ReadyPacket);
        }
        public void SendASWCRequestPacket()
        {
            this.Write(this.ASWCRequestPacket);
        }

        /// <summary>
        /// Method to extract board versioning info from intro response packets
        /// </summary>
        /// <param name="packet">The received packet</param>
        /// <returns>True of intro packet, else false</returns>
        protected virtual bool ParseIntroPacket(byte[] packet)
        {
            Util.TestConsoleWrite(this.TestMode, "Parsing for intro packet!");
            String content = String.Empty;
            foreach (byte b in packet)
            {
                content += Convert.ToChar(b);
            }

            if (content.Substring(6, 3).Equals("CWI"))
            {
                this.ProductID = content.Substring(6, 9);
                this.AppFirmwareVersion = content.Substring(25, 3);
                Util.TestConsoleWrite(this.TestMode, "PID: " + this.ProductID);
                Util.TestConsoleWrite(this.TestMode, "App ver: " + this.AppFirmwareVersion);
                return true;
            }
            else { return false; }
        }
        protected virtual bool ProcessIntroPacket(byte[] packet)
        {
            //43, 57, 49 == CWI
            if (packet.Length > 9 
                && packet[6] == 0x43 
                && packet[7] == 0x57 
                && packet[8] == 0x49)
                return this.ParseIntroPacket(packet);
            else
                return false;
        }

        public virtual byte[] PrepPacket(byte[] packet) { return packet; }

        //Event related stuff
        public virtual bool IsAck(byte[] packet) 
        {
            if (packet[packet.Length - 1] == 0x41)
            {
                Util.TestConsoleWrite(this.TestMode, "Ack received!");
                return true;
            }
            else { return false; }
        }
        public virtual bool IsFinal(byte[] packet) 
        {
            return false;
        }

        /// <summary>
        /// This method is called asynchronously when a packet is received.
        /// It will forward the packet to the appropriate event handler based on identification
        /// </summary>
        /// <param name="oInRep">The input report containing the packet</param>
        protected override void HandleDataReceived(byte[] packet)
        {
            Util.TestConsoleWrite(this.TestMode, "Handling received packet...");
            if (TestMode)
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in packet)
                {
                    sb.Append(Convert.ToChar(b));
                }
                Util.TestConsoleWrite(TestMode, sb.ToString());
            }

            if (this.ProductID.Equals(String.Empty))
            {
                if (this.ProcessIntroPacket(packet))
                    this.OnIntroReceived(new PacketEventArgs(packet));
            }
            else if (this.IsAck(packet)) this.OnAckReceived(new PacketEventArgs(packet));
            else if (this.IsFinal(packet)) this.OnFinalReceived(new PacketEventArgs(packet));
        }

        public InputReport CreateInputReport()
        {
            return new AxxessInputReport(this);
        }

        public virtual byte CalculateChecksum(byte[] packet)
        {
            return 0x00;
        }

        #region Events
        public event IntroEventHandler OnIntro;
        public event AckEventHandler OnAck;
        public event FinalEventHandler OnFinal;
        public event ASWCInfoHandler OnASWCInfo;

        public virtual void OnIntroReceived(EventArgs e) { if (OnIntro != null) OnIntro(this, e); }
        public virtual void OnAckReceived(EventArgs e) { if (OnAck != null) OnAck(this, e); }
        public virtual void OnFinalReceived(EventArgs e) { if (OnFinal != null) OnFinal(this, e); }
        public virtual void OnASWCInfoReceieved(EventArgs e) { if (OnASWCInfo != null) OnASWCInfo(this, e); }
        #endregion 

        #region Explicit IAxxessDevice Implementation
        string IAxxessBoard.ProductID { get { return this.ProductID; } }
        string IAxxessBoard.AppFirmwareVersion { get { return this.AppFirmwareVersion; } }
        string IAxxessBoard.BootFirmwareVersion { get { return this.BootFirmwareVersion; } }
        string IAxxessBoard.Info { get { return this.Info; } }
        BoardType IAxxessBoard.Type { get { return this.Type; } }
        int IAxxessBoard.PacketSize { get { return this.PacketSize; } }

        byte[] IAxxessBoard.PrepPacket(byte[] packet) { return this.PrepPacket(packet); }
        byte[] IAxxessBoard.IntroPacket { get { return this.IntroPacket; } }
        byte[] IAxxessBoard.ReadyPacket { get { return this.ReadyPacket; } }
        
        void IAxxessBoard.SendIntroPacket() { this.SendIntroPacket(); }
        void IAxxessBoard.SendReadyPacket() { this.SendReadyPacket(); }
        void IAxxessBoard.SendASWCMappingPacket(ASWCButtonMap map) { throw new NotImplementedException(); }
        void IAxxessBoard.SendASWCRequestPacket() { this.SendASWCRequestPacket(); }
        void IAxxessBoard.SendPacket(byte[] packet) { this.Write(packet); }
        void IAxxessBoard.SendRawPacket(byte[] packet) { this.Write(packet); }
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

        void IAxxessBoard.AddASWCConfimEvent(ASWCConfirmHandler handler)
        {
            throw new NotImplementedException();
        }

        void IAxxessBoard.RemoveASWCConfimEvent(ASWCConfirmHandler handler)
        {
            throw new NotImplementedException();
        }

        void IAxxessBoard.Dispose() { this.Dispose(); }

        #endregion
    }
}