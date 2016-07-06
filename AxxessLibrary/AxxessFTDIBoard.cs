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
    /// Class represents an Axxess board using the FTDI chipset for communication.
    /// </summary>
    public class AxxessFTDIBoard : IAxxessBoard
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
                sb.AppendFormat("Product ID: {0}, App Ver: {1}, Boot Ver: {2}, Baud Rate: {3}", 
                    this.ProductID, this.AppFirmwareVersion, this.BootFirmwareVersion, this.BaudRate.ToString());
                return sb.ToString();
            }
        }
        private BoardStatus Status { get; set; }        

        public byte[] IntroPacket { get; protected set; }
        public byte[] ReadyPacket { get; protected set; }
        public byte[] ASWCRequestPacket { get; protected set; }

        public BoardType Type { get; protected set; }
        /// <summary>
        /// The cable connecting this device to the computer.
        /// </summary>
        public FTDICable FTDIDevice { get; protected set; }
        /// <summary>
        /// Which baud rate is being used?
        /// </summary>
        public uint BaudRate { get; protected set; }

        /// <summary>
        /// When this is set to true, the system will stop reading packets from the device.
        /// </summary>
        public bool StopRead { get; set; }

        internal AxxessFTDIBoard(FTDICable connector, uint baudRate)
        {
            this.ProductID = String.Empty;
            this.AppFirmwareVersion = String.Empty;
            this.BootFirmwareVersion = String.Empty;
            this.Status = BoardStatus.Idle;

            this.PacketSize = 44;

            this.IntroPacket = new byte[] { 0x01, 0xF0, 0x10, 0x03, 0xA0, 0x01, 0x0F, 0x58, 0x04 };
            this.ReadyPacket = new byte[] { 0x01, 0xF0, 0x20, 0x00, 0xEB, 0x04 };

            this.FTDIDevice = connector;
            BaudRate = baudRate;

            StopRead = false;

            this.Initialize();
        }

        protected virtual void Initialize()
        {
            this.Type = BoardType.FTDI;
            this.OnIntro += ParseIntroPacket;
            this.FTDIDevice.OpenPortForAxxess(this.BaudRate);
            this.BeginAsyncRead();
        }

        //Atomic packet operations
        public void Write(byte[] packet)
        {
            uint resp = this.FTDIDevice.WriteToPort(packet);
            Console.WriteLine("Wrote {0} bytes!", resp);
            if (resp == 0)
                throw new IOException("Failed to write to FTDI device!");
        }

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
            throw new NotSupportedException("ASWC protocols are not supported by the board type!");
        }
        public virtual byte[] PrepPacket(byte[] packet) 
        {
            byte[] newPacket = new byte[44];
            byte[] content = packet;

            //Add content bytes
            for (int i = 0; i < content.Length; i++)
                newPacket[i] = content[i];

            return newPacket;
        }

        /// <summary>
        /// Method to extract board versioning info from intro response packets.
        /// </summary>
        /// <param name="packet">The received packet</param>
        /// <returns>True of intro packet, else false</returns>
        protected virtual void ParseIntroPacket(object sender, PacketEventArgs args)
        {
            //Parse packet into characters
            byte[] packet = args.Packet;
            StringBuilder sb = new StringBuilder();
            foreach (byte b in packet)
            {
                char c = Convert.ToChar(b);
                if (Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                }
            }
            string content = sb.ToString();

            if (content.Equals(String.Empty) || !content.Contains("CWI"))
                return;

            string[] words = content.Split(new char[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);

            this.ProductID = words[0];
            this.BootFirmwareVersion = words[1];
            if (words.Length > 2)
                this.AppFirmwareVersion = words[2];
            this.OnIntro -= ParseIntroPacket;
            return;
        }     

        //Event related stuff
        public virtual bool IsAck(byte[] packet) 
        {
            return false;
        }
        public virtual bool IsFinal(byte[] packet) 
        { 
            return false; 
        }

        /// <summary>
        /// This method is called asynchronously when a packet is received.
        /// It will forward the packet to the appropriate event handler based on identification
        /// </summary>
        /// <param name="packet">An array of bytes representing an input packet.</param>
        /// <remarks>
        /// Notable packets:
        ///     Ack ready to start update: 01 60 01 0F 20 00 CC 04 00
        ///     Ack ready for next: 01 60 39 39 41
        ///     Ack update completed: 01 60 38 38
        /// </remarks>
        protected virtual void HandleDataReceived(byte[] packet)
        {
            if (OnPacket != null)
            {
                OnPacketReceived(new PacketEventArgs(packet));
            }
            if (OnIntro != null)
            {
                OnIntroReceived(new PacketEventArgs(packet));
            }
        }
        protected virtual void HandleDeviceRemoved()
        {

        }

        /// <summary>
        /// Begins an asynch read operation.
        /// </summary>
        private void BeginAsyncRead()
        {
            if (FTDIDevice.IsPortOpen)
            {
                byte[] buffer = new byte[this.PacketSize];
                FTDIDevice.BeginRead(buffer, ReadCompleted);
            }
        }

        /// <summary>
        /// Callback used at the end of the read operation.
        /// </summary>
        /// <param name="result">The result of the read op.</param>
        public void ReadCompleted(IAsyncResult result)
        {
            try
            {
                if (result.IsCompleted)
                {
                    HandleDataReceived((byte[])result.AsyncState);
                }
                else
                {
                    HandleDeviceRemoved();
                }
            }
            finally 
            {
                if (!StopRead)
                {
                    BeginAsyncRead();
                }
            }
        }

        public void Dispose()
        {
            if (this.FTDIDevice.IsOpen)
                this.FTDIDevice.CloseCommPort();
        }

        public virtual byte CalculateChecksum(byte[] packet)
        {
            return 0x00;
        }
        
        #region Events
        public event IntroEventHandler OnIntro;
        public event AckEventHandler OnAck;
        public event FinalEventHandler OnFinal;
        //public event ASWCInfoHandler OnASWCInfo;
        public event EventHandler OnDeviceRemoved;
        public event PacketHandler OnPacket;

        public virtual void OnIntroReceived(PacketEventArgs e) { OnIntro(this, e); }
        public virtual void OnAckReceived(EventArgs e) { OnAck(this, e); }
        public virtual void OnFinalReceived(EventArgs e) { OnFinal(this, e); }
        //public virtual void OnASWCInfoReceieved(EventArgs e) { if (OnASWCInfo != null) OnASWCInfo(this, e); }
        public virtual void OnPacketReceived(PacketEventArgs e) { OnPacket(this, e); }
        #endregion 

        #region Explicit IAxxessDevice Implementation
        string IAxxessBoard.ProductID { get { return this.ProductID; } }
        string IAxxessBoard.AppFirmwareVersion { get { return this.AppFirmwareVersion; } }
        string IAxxessBoard.BootFirmwareVersion { get { return this.BootFirmwareVersion; } }
        string IAxxessBoard.Info { get { return this.Info; } }
        ASWCInfo IAxxessBoard.ASWCInformation { get { throw new NotImplementedException(); } }
        BoardType IAxxessBoard.Type { get { return this.Type; } }
        int IAxxessBoard.PacketSize { get { return this.PacketSize; } }

        byte[] IAxxessBoard.PrepPacket(byte[] packet) { return this.PrepPacket(packet); }
        byte[] IAxxessBoard.IntroPacket { get { return this.IntroPacket; } }
        byte[] IAxxessBoard.ReadyPacket { get { return this.ReadyPacket; } }

        void IAxxessBoard.SendIntroPacket() { this.SendIntroPacket(); }
        void IAxxessBoard.SendReadyPacket() { this.SendReadyPacket(); }
        void IAxxessBoard.SendASWCMappingPacket(ASWCInfo map, IList<SectionChanged> list) { throw new NotImplementedException(); }
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
        void IAxxessBoard.AddASWCInfoEvent(ASWCInfoHandler handler) { return; }
        void IAxxessBoard.RemoveASWCInfoEvent(ASWCInfoHandler handler) { return; }
        void IAxxessBoard.AddPacketEvent(PacketHandler handler) { this.OnPacket += handler; }
        void IAxxessBoard.RemovePacketEvent(PacketHandler handler) { this.OnPacket -= handler; }

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