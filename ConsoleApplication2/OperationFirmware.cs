using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Metra.Axxess
{
    class OperationFirmware : Operation
    {
        AxxessFirmware File { get; set; }
        IEnumerator<byte[]> _fileEnum;

        bool AckRx { get; set; }
        bool FinalRx { get; set; }

        public OperationFirmware(IAxxessBoard device, AxxessFirmware file) : base(device, OperationType.Firmware)
        {
            this.File = file;
            this._fileEnum = this.File.GetEnumerator();
            this.TotalOperations = this.File.Count;
        }

        public override void DoWork()
        {
            this.Work();
        }

        public override void Work()
        {
            base.Work();

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
        /// Procedural
        /// </summary>
        private void WorkForCDC()
        {
            Timeout = 10;
            _timeoutCounter = 0;
            CanTimeOut = true;

            //Register events
            //this.Device.AddAckEvent(AckHandler);
            //this.Device.AddFinalEvent(FinalHandler);

            AxxessCDCBoard board = (AxxessCDCBoard)Device;

            //Turns off the board's built in async read so that we can 
            board.StopRead = true;
            //board.Port.DiscardInBuffer();
            //board.Port.DiscardOutBuffer();
            //board.Port.Open();
            Thread.Sleep(100);
            
            //Wait for ACK byte
            Timeout = 10;
            _timeoutCounter = 0;

            board.SendReadyPacket();
            while (!IsTimedOut)
            {
                //Thread.Sleep(100);
                try
                {
                    int bytes = board.Port.BytesToRead;
                    if (bytes > 0)
                    {
                        byte[] buffer = new byte[bytes];
                        board.Port.Read(buffer, 0, bytes);
                        foreach (byte b in buffer)
                        {
                            Console.Write("{0} ", b);
                            if (b == 0x41)
                            {
                                //Console.WriteLine("Ack!");
                                goto filetrans;
                            }

                        }
                        Console.WriteLine();
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
                //Thread.Sleep(100);
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
                                Console.Write("{0} ", b);
                                if (b == 0x41)
                                {
                                    //Console.WriteLine("Ack!");
                                    ackRX = true;
                                    break;
                                }
                                else if (b == 0x38)
                                {
                                    //Console.WriteLine("Final!");
                                    goto endtrans;
                                }

                            }
                            Console.WriteLine();
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
                while (!this.AckRx && this.Status.Equals(OperationStatus.Working))
                {
                    //Thread.Sleep(10);
                }
                
                _fileEnum.MoveNext();
                this.ResetFlags();
                this.Device.SendPacket(_fileEnum.Current);
                this.OperationsCompleted++;
            }

            while (!this.AckRx && this.Status.Equals(OperationStatus.Working))
            {
                //Thread.Sleep(10);
            }

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

        private void WorkForHID()
        {
            //Register events
            this.Device.AddAckEvent(AckHandler);
            this.Device.AddFinalEvent(FinalHandler);

            //Send the ready packet and wait for reply
            this.Device.SendReadyPacket();
        }

        private void WorkForFTDI()
        {
            Timeout = 10;
            _timeoutCounter = 0;
            CanTimeOut = true;

            //Register events
            //this.Device.AddAckEvent(AckHandler);
            //this.Device.AddFinalEvent(FinalHandler);

            AxxessFTDIBoard board = (AxxessFTDIBoard)Device;
            
            //Turns off the board's built in async read so that we can 
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

        public void AckHandler(object sender, EventArgs e)
        {
            if (this.Status.Equals(OperationStatus.Working) && this._fileEnum.MoveNext())
            {
                this.Device.SendPacket(_fileEnum.Current);
                this.OperationsCompleted++;
            }
        }

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

        public void FinalHandler(object sender, EventArgs e)
        {
            this.OperationsCompleted++;
            this.Status = OperationStatus.Finished;
            this.Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();

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
        }

        //Flag events
        public void AckRxHandler(object sender, EventArgs e)
        {
            this.AckRx = true;
        }

        public void FinalRxHandler(object sender, EventArgs e)
        {
            this.FinalRx = true;
        }

        public void ResetFlags()
        {
            this.AckRx = false;
            this.FinalRx = false;
        }
    }
}
