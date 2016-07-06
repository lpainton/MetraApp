using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Metra.Axxess
{
    /// <summary>
    /// Contains logic for updating the firmware on each board type.
    /// </summary>
    class OperationFirmware : Operation
    {
        /// <summary>
        /// The firmware file object being used to update the board.
        /// </summary>
        AxxessFirmware File { get; set; }

        /// <summary>
        /// A reference to the firmware file's enumerator object.
        /// </summary>
        IEnumerator<byte[]> _fileEnum;

        /// <summary>
        /// Flag set to true if ack received.
        /// </summary>
        //bool AckRx { get; set; }

        /// <summary>
        /// Flag set to true if final packet received.
        /// </summary>
        //bool FinalRx { get; set; }

        /// <summary>
        /// Semaphore used while waiting on device for ack
        /// </summary>
        EventWaitHandle WaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        internal OperationFirmware(IAxxessBoard device, AxxessFirmware file) : base(device, OperationType.Firmware)
        {
            this.File = file;
            this._fileEnum = this.File.GetEnumerator();
            this.TotalOperations = this.File.Count;
        }

        public override void Stop()
        {
            base.Stop();
            WaitHandle.Set();
        }

        /// <summary>
        /// Overriden version of the worker method runs Work only once.
        /// </summary>
        public override void DoWork()
        {
            this.Work();
        }

        /// <summary>
        /// Work here is a helper method, branching off to a case based on the board type.
        /// </summary>
        public override void Work()
        {
            base.Work();

            this.Message = "Updating to firmware version " + this.File.Version;

            switch (Device.Type)
            {
                case BoardType.FTDI:
                    WorkForFTDI();
                    break;
                
                case BoardType.HIDThree:
                    WorkForHID3();
                    break;

                case BoardType.MicrochipCDC:
                    WorkForCDC();
                    break;

                default:
                    WorkForHID();
                    break;
            }
        }

        /// <summary>
        /// Procedural update method which directly controls the update process for CDC boards.
        /// </summary>
        private void WorkForCDC()
        {
            Timeout = 10;
            _timeoutCounter = 0;
            CanTimeOut = true;

            AxxessCDCBoard board = (AxxessCDCBoard)Device;

            //Turns off the board's built in async read so that we can control the read directly.
            board.StopRead = true;
            Thread.Sleep(100);
            
            //Wait for ACK byte
            Timeout = 10;
            _timeoutCounter = 0;

            board.SendReadyPacket();
            while (!IsTimedOut)
            {
                try
                {
                    int bytes = board.Port.BytesToRead;
                    if (bytes > 0)
                    {
                        byte[] buffer = new byte[bytes];
                        board.Port.Read(buffer, 0, bytes);
                        foreach (byte b in buffer)
                        {
                            //Here we are looking for an ack byte in the data stream.
                            if (b == 0x41)
                            {
                                //Goto here breaks out of nested loop.
                                goto filetrans;
                            }

                        }
                    }
                }
                catch (TimeoutException)
                {
                    IncrementTimeout();
                }

            }
            if (IsTimedOut)
            {
                Error = new TimeoutException("Firmware operation did not receive an ack from the board!");
                this.Stop();
                return;
            }

            
            //Send packets in 44 byte chunks
            filetrans:
            while (this.Status.Equals(OperationStatus.Working) && this._fileEnum.MoveNext())
            {
                bool ackRX = false;
                board.Write(_fileEnum.Current);
                OperationsCompleted++;
   
                Timeout = 10;
                _timeoutCounter = 0;

                while (!ackRX && !IsTimedOut)
                {
                    try
                    {
                        int bytes = board.Port.BytesToRead;
                        if (bytes > 0)
                        {
                            byte[] buffer = new byte[bytes];
                            board.Port.Read(buffer, 0, bytes);
                            foreach (byte b in buffer)
                            {
                                //Waiting for ack or final
                                if (b == 0x41)
                                {
                                    ackRX = true;
                                    break;
                                }
                                else if (b == 0x38)
                                {
                                    //Used a goto here because the nested loops make breaking difficult
                                    //C# lacks a labelled break to address this problem.
                                    goto endtrans;
                                }

                            }
                        }
                    }
                    catch (TimeoutException)
                    {
                        IncrementTimeout();
                    }
                }
                if (IsTimedOut)
                {
                    Error = new TimeoutException("Firmware operation did not receive transfer ack from the board!");
                    this.Stop();
                    return;
                }
            }

        endtrans:
            this.Stop();

        }


        //Semaphore events used in HID-3
        public void AckRxHandler(object sender, EventArgs e) { WaitHandle.Set(); }
        public void FinalRxHandler(object sender, EventArgs e) { WaitHandle.Set(); }
        /*public void ResetFlags()
        {
            this.AckRx = false;
            this.FinalRx = false;
        }*/
        /// <summary>
        /// Also known as the HID 293 board.  This board has some peculiarities that set it apart from normal HID updates.
        /// </summary>
        private void WorkForHID3()
        {
            //Register events
            this.Device.AddAckEvent(AckRxHandler);
            this.Device.AddFinalEvent(FinalRxHandler);

            //Send the ready packet and wait for reply 
            this.Device.SendReadyPacket();

            //Wait for Rx
            while (this.File.Index < (this.File.Count - 2) && this.Status.Equals(OperationStatus.Working))
            {
                //Spin lock is the result of some messy planning.
                /*while (!this.AckRx && this.Status.Equals(OperationStatus.Working))
                {

                }*/
                WaitHandle.WaitOne();
                
                _fileEnum.MoveNext();
                //this.ResetFlags();
                this.Device.SendPacket(_fileEnum.Current);
                this.OperationsCompleted++;
            }

            //Another spin lock.  TODO: Eliminate this lock and its ugly cousin
            /*while (!this.AckRx && this.Status.Equals(OperationStatus.Working))
            {
            }*/
            WaitHandle.WaitOne();

            if (this.Status.Equals(OperationStatus.Working))
            {
                _fileEnum.MoveNext();
                this.OperationsCompleted++;
                byte[] packet = new byte[65];
                byte[] content = _fileEnum.Current;
                for (int i = 0; i < content.Length; i++)
                {
                    packet[i + 2] = content[i];
                }
                packet[1] = 0xFF;
                packet[64] = this.Device.CalculateChecksum(content);
                this.Device.SendRawPacket(packet);

                this.OperationsCompleted++;
                this.Status = OperationStatus.Finished;
            }
            this.Dispose();
        }

        //Events for HID updating.
        public void AckHandler(object sender, EventArgs e)
        {
            if (this.Status.Equals(OperationStatus.Working) && this._fileEnum.MoveNext())
            {
                this.Device.SendPacket(_fileEnum.Current);
                this.OperationsCompleted++;
            }
        }
        public void FinalHandler(object sender, EventArgs e)
        {
            this.OperationsCompleted++;
            this.Status = OperationStatus.Finished;
            this.Dispose();
        }
        /// <summary>
        /// The normal HID firmware update is purely event driven.
        /// Thus the board itself controls the flow of the firmware update.
        /// </summary>
        /// <remarks>
        /// I haven't been thrilled with the speed of updates using this method.
        /// Turns out the firing thread on these events is the asynch read thread, which
        /// winds up slowing things down because it winds up waiting for the packet to send
        /// before terminating.
        /// 
        /// This is probably best resolved by switching to a producer/consumer model on device
        /// communications management.  Basically set a consumer thread on a semaphore and signal
        /// it when the device receives a valid packet.  Have it consume all incoming packets
        /// from a queue and call events accordingly.
        /// </remarks>
        private void WorkForHID()
        {
            //Register events
            this.Device.AddAckEvent(AckHandler);
            this.Device.AddFinalEvent(FinalHandler);

            //Send the ready packet and wait for reply
            this.Device.SendReadyPacket();
        }

        /// <summary>
        /// Procedural method for FTDI update process.
        /// </summary>
        private void WorkForFTDI()
        {
            Timeout = 10;
            _timeoutCounter = 0;
            CanTimeOut = true;

            AxxessFTDIBoard board = (AxxessFTDIBoard)Device;
            
            //Turns off the board's built in async read so that we can 
            //manually control read operations
            board.StopRead = true;


            uint respBytes = 0;
            byte[] packet = new byte[6];

            if (board.FTDIDevice.IsPortOpen)
            {
                board.FTDIDevice.CloseCommPort();
                board.FTDIDevice.OpenPortForAxxess(board.BaudRate);
            }


            while(!IsTimedOut)
            {

                this.Device.SendReadyPacket();
                Thread.Sleep(100);
                respBytes = board.FTDIDevice.ReadFromPort(packet, 6);
                //board.FTDIDevice.CloseCommPort();

                if (respBytes > 0 && packet[2] == 0x20) break;
                else IncrementTimeout();
            }
            if (IsTimedOut)
            {
                Error = new TimeoutException("Firmware operation did not receive a response from the board!");
                this.Stop();
                return;
            } 
            
            //Wait for ACK byte
            board.FTDIDevice.SetTimeouts(4000, 4000);
            Timeout = 10;
            _timeoutCounter = 0;
            respBytes = 0;
            packet = new byte[1];
            while(!IsTimedOut)
            {
                respBytes = board.FTDIDevice.ReadFromPort(packet, 1);
                if (respBytes > 0 && packet[0] == 0x41) break;
                else IncrementTimeout();
            }
            if (IsTimedOut)
            {
                Error = new TimeoutException("Firmware operation did not receive an ack from the board!");
                this.Stop();
                return;
            } 

            //Send packets in 44 byte chunks
            packet = new byte[2];
            while (this.Status.Equals(OperationStatus.Working) && this._fileEnum.MoveNext())
            {
                board.FTDIDevice.PurgeForAxxess();
                board.Write(_fileEnum.Current);
                OperationsCompleted++;
                Timeout = 10;
                _timeoutCounter = 0;
                respBytes = 0;
                packet = new byte[2];

                while(!IsTimedOut)
                {
                    respBytes = board.FTDIDevice.ReadFromPort(packet, 2);
                    if (respBytes > 0)
                        if (packet[0] < 0x39)
                            if (packet[0] <= 0x38)
                                break;
                        if (packet[1] == 0x39) break;
                    else IncrementTimeout();
                }
                if (IsTimedOut)
                {
                    Error = new TimeoutException("Firmware operation did not receive transfer ack from the board!");
                    this.Stop();
                    return;
                }
            }  
            
            if (packet[0] == 0x38)
            {
                this.Stop();
            }
        }

        /// <summary>
        /// Not being used.
        /// </summary>
        public void CDCAckHandler(object sender, EventArgs e)
        {
            if (this.Status.Equals(OperationStatus.Working))
            {
                if (_fileEnum.MoveNext())
                {
                    this.Device.SendPacket(_fileEnum.Current);
                    this.OperationsCompleted++;
                }
                else
                {
                    this.FinalHandler(sender, e);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            this.Message = "Update to firmware version " + this.File.Version + " completed.";

            if (Device.Type.Equals(BoardType.HIDChecksum) || Device.Type.Equals(BoardType.HIDNoChecksum))
            {
                this.Device.RemoveAckEvent(AckHandler);
                this.Device.RemoveFinalEvent(FinalHandler);
            }
            else if (Device.Type.Equals(BoardType.HIDThree))
            {
                this.Device.RemoveAckEvent(AckRxHandler);
                this.Device.RemoveFinalEvent(FinalRxHandler);
            }

            OnCompleted();
        }
    }
}
