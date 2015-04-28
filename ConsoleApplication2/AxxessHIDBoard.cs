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

        public int ProductID { get; protected set; }
        public int AppFirmwareVersion { get; protected set; }
        public int BootFirmwareVersion { get; protected set; }
        private BoardStatus Status { get; set; }        

        public byte[] IntroPacket { get; protected set; }
        public byte[] ReadyPacket { get; protected set; }
        public byte[] ASWCRequestPacket { get; protected set; }

        public BoardType Type { get; protected set; }

        public AxxessHIDBoard()
            : base()
        {
            this.ProductID = 0;
            this.AppFirmwareVersion = 0;
            this.BootFirmwareVersion = 0;
            this.Status = BoardStatus.Idle;

            this.PacketSize = 44;
            this.IntroPacket = this.PrepPacket(new byte[] { 0x01, 0xF0, 0x10, 0x03, 0xA0, 0x01, 0x0F, 0x58, 0x04 });
            this.ReadyPacket = this.PrepPacket(new byte[] { 0x01, 0xF0, 0x20, 0x00, 0xEB, 0x04 });
            this.ASWCRequestPacket = this.PrepPacket(new byte[] { 0x01, 0xF0, 0xA0, 0x03, 0x10, 0x01, 0x00, 0x57, 0x04 });

            this.Initialize();
        }

        protected virtual void Initialize()
        {
            this.Type = BoardType.HIDNoChecksum;
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
        public void SendASWCRequestPacket()
        {
            this.Write(new ASWCRequestReport(this));
        }
        public virtual byte[] PrepPacket(byte[] packet) 
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

            return newPacket;
        }
        protected virtual bool ProcessIntroPacket(byte[] packet) { return false; }

        //Event related stuff
        public virtual bool IsAck(byte[] packet) { return false; }
        public virtual bool IsFinal(byte[] packet) { return false; }

        /// <summary>
        /// This method is called asynchronously when a packet is received.
        /// It will forward the packet to the appropriate event handler based on identification
        /// </summary>
        /// <param name="oInRep">The input report containing the packet</param>
        protected override void HandleDataReceived(InputReport InRep)
        {
            base.HandleDataReceived(InRep);

            byte[] packet = InRep.Buffer;

            if (this.ProductID == 0)
            {
                if (this.ProcessIntroPacket(packet))
                    this.OnIntroReceived(new PacketEventArgs(packet));
            }
            else if (this.IsAck(packet)) this.OnAckReceived(new PacketEventArgs(packet));
            else if (this.IsFinal(packet)) this.OnFinalReceived(new PacketEventArgs(packet));
        }

        public override InputReport CreateInputReport()
        {
            return new AxxessInputReport(this);
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
        int IAxxessBoard.ProductID { get { return this.ProductID; } }
        int IAxxessBoard.AppFirmwareVersion { get { return this.AppFirmwareVersion; } }
        int IAxxessBoard.BootFirmwareVersion { get { return this.BootFirmwareVersion; } }
        int IAxxessBoard.PacketSize { get { return this.PacketSize; } }

        byte[] IAxxessBoard.PrepPacket(byte[] packet) { return this.PrepPacket(packet); }
        byte[] IAxxessBoard.IntroPacket { get { return this.IntroPacket; } }
        byte[] IAxxessBoard.ReadyPacket { get { return this.ReadyPacket; } }

        void IAxxessBoard.SendIntroPacket() { this.SendIntroPacket(); }
        void IAxxessBoard.SendReadyPacket() { this.SendReadyPacket(); }
        void IAxxessBoard.SendASWCMappingPacket(ASWCButtonMap map) { throw new NotImplementedException(); }
        void IAxxessBoard.SendASWCRequestPacket() { this.SendASWCRequestPacket(); }
        void IAxxessBoard.SendPacket(byte[] packet) { this.Write(new GenericOutputReport(this, packet)); }

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

        #endregion
    }
}