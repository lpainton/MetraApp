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
    public delegate bool PacketHandler(byte[] packet);

    public abstract class HIDAxxessBoard : HIDDevice, IAxxessDevice
    {
        //Stores delegates to be called for packet handling
        //private List<PacketHandler> _packetChain;
        private Queue<PacketType> _packetQueue;


        //Board attributes
        public int ProductID { get; protected set; }
        public int AppFirmwareVersion { get; protected set; }
        public int BootFirmwareVersion { get; protected set; }
        private BoardStatus Status { get; set; }

        public byte[] IntroPacket { get; protected set; }
        public byte[] ReadyPacket { get; protected set; }

        public BoardType Type { get; protected set; }

        public HIDAxxessBoard()
            : base()
        {
            this.ProductID = 0;
            this.AppFirmwareVersion = 0;
            this.BootFirmwareVersion = 0;
            this.Status = BoardStatus.Idle;
            //_packetChain = new List<PacketHandler>();
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
        public virtual byte[] PrepPacket(byte[] packet) { return packet; }
        protected virtual bool ProcessIntroPacket(byte[] packet) { return false; }

        //Event related stuff
        protected virtual bool IsAck(byte[] packet) { return false; }
        protected virtual bool IsFinal(byte[] packet) { return false; }

        //Packet chain
        /*protected virtual void AddHandler(PacketHandler handler)
        {
            this._packetChain.Add(handler);
        }*/

        /// <summary>
        /// This method is called asynchronously when a packet is received.
        /// Depending on the board status it will call whatever is in the _packetHandlers
        /// dictionary keyed on BoardStatus.
        /// 
        /// To change the behavior of this method it's best to change the delegates assigned
        /// to each board status.
        /// </summary>
        /// <param name="oInRep">The input report containing the packet</param>
        protected override void HandleDataReceived(InputReport InRep)
        {
            base.HandleDataReceived(InRep);

            /*List<PacketHandler> nChain = new List<PacketHandler>();
            foreach (PacketHandler h in this._packetChain)
            {
                if (h(InRep.Buffer))
            }*/
            lock (this._packetQueue)
            {
                byte[] packet = InRep.Buffer;
                if (this.ProcessIntroPacket(packet))
                    this._packetQueue.Enqueue(PacketType.Intro);
                else if (this.IsAck(packet))
                    this._packetQueue.Enqueue(PacketType.Ack);
                else if (this.IsFinal(packet))
                    this._packetQueue.Enqueue(PacketType.Final);
            }
        }

        //Functional Logic
        public virtual void StartForceIdle();
        public virtual void UpdateAppFirmware(string path, ToolStripProgressBar bar) { return; }

        #region Statics
        public static HIDAxxessBoard ConnectToBoard()
        {
            HIDAxxessBoard device;
            device = (HIDAxxessBoard)HIDDevice.FindDevice(1240, 63301, typeof(HIDChecksumBoard));

            return device;
        }
        #endregion

        #region Explicit IAxxessDevice Implementation
        int IAxxessDevice.ProductID
        {
            get { return this.ProductID; }
        }
        int IAxxessDevice.AppFirmwareVersion
        {
            get { return this.AppFirmwareVersion; }
        }
        int IAxxessDevice.BootFirmwareVersion
        {
            get { return this.BootFirmwareVersion; }
        }
        void IAxxessDevice.StartForceIdle()
        {
            this.StartForceIdle();
        }
        byte[] IAxxessDevice.PrepPacket(byte[] packet) { return this.PrepPacket(packet); }
        byte[] IAxxessDevice.IntroPacket { get { return this.IntroPacket; } }
        byte[] IAxxessDevice.ReadyPacket { get { return this.ReadyPacket; } }
        #endregion
    }
}