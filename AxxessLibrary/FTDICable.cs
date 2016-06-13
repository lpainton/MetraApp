using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FTD2XX_NET;

namespace Metra.Axxess
{
    public class FTDIException : Exception
    {
        public FTDIException(FTDI.FT_STATUS status) : base(status.ToString()) { }
    }

    public class FTDIAsynchReadResult : IAsyncResult
    {
        public object State { get; set; }
        public bool IsCompleted { get; set; }

        public FTDIAsynchReadResult()
        {
            State = null;
            IsCompleted = false;
        }

        object IAsyncResult.AsyncState
        {
            get { return this.State; }
        }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get { throw new NotImplementedException(); }
        }

        bool IAsyncResult.CompletedSynchronously
        {
            get { throw new NotImplementedException(); }
        }

        bool IAsyncResult.IsCompleted
        {
            get { return this.IsCompleted; }
        }
    }

    public class FTDICable : FTDI
    {
        public bool IsPortOpen { get; private set; }

        public FTDICable() : base()
        {
            this.IsPortOpen = false;
        }

        public uint SearchBaudRate(uint[] rates, uint timeout)
        {
            uint resp = 0;
            Queue<uint> baudRates = new Queue<uint>(rates);
            uint counter = 0;

            this.SetTimeouts(100, 50);
            this.SetLatency(2);

            while (resp == 0)
            {
                baudRates.Enqueue(baudRates.Dequeue());
                SetBaudRate(baudRates.Peek());
                WriteToPort(new byte[] { 0x01, 0xF0, 0x10, 0x03, 0xA0, 0x01, 0x0F, 0x58, 0x04 });
                Thread.Sleep(20);
                GetRxBytesAvailable(ref resp);

                counter++;
                if (counter == timeout)
                {
                    return 0;
                }
            }

            return baudRates.Peek();
        }

        public void OpenPortForAxxess(uint baudRate)
        {
            OpenCommPort(115200, FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
        }

        private void ValidateStatus(FTDI.FT_STATUS status)
        {
            if (status != FTDI.FT_STATUS.FT_OK)
                throw new FTDIException(status);
        }

        public uint WriteToPort(byte[] packet)
        {
            UInt32 numBytesWritten = 0;
            this.Write(packet, packet.Length, ref numBytesWritten);
            return numBytesWritten;
        }

        public uint ReadFromPort(byte[] buffer, uint numBytes = 44)
        {
            UInt32 numBytesRead = 0;
            this.Read(buffer, numBytes, ref numBytesRead);
            return numBytesRead;
        }

        public void BeginRead(byte[] buffer, AsyncCallback callback)
        {
            FTDIAsynchReadResult result = new FTDIAsynchReadResult();
            Thread ReadWorker = new Thread(o => {
                
                int counter = 0;
                int maxReads = 5;
                FTDIAsynchReadResult res = (FTDIAsynchReadResult)o;
                res.IsCompleted = false;

                while (!res.IsCompleted && counter <= maxReads)
                {
                    uint numBytes = ReadFromPort(buffer);
                    res.State = buffer;
                    res.IsCompleted = (numBytes > 0) && (res.State != null);
                    //Console.WriteLine("Received {0} bytes...", numBytes);
                    counter++;
                }                   
                callback((IAsyncResult)res);
            });
            ReadWorker.Start(result);
        }

        public void CloseCommPort() 
        { 
            ValidateStatus(this.Close());
            this.IsPortOpen = false;
        }

        //Bauds are 19200 or 115200
        public void OpenCommPort(uint baudRate, byte wordLen, byte stopBits, byte parity)
        {
            Console.WriteLine("Open port by index...");
            ValidateStatus(this.OpenByIndex(0));

            //Console.WriteLine("Resetting...");
            ValidateStatus(this.ResetDevice());

            //Console.WriteLine("Reset device...");
            ValidateStatus(this.ResetDevice());

            //Console.WriteLine("Set baud rate...");
            ValidateStatus(this.SetBaudRate(baudRate));

            //Console.WriteLine("Set data characteristics...");
            ValidateStatus(this.SetDataCharacteristics(wordLen, stopBits, parity));

            //Console.WriteLine("Set flow control...");
            ValidateStatus(this.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0x11, 0x19));

            //Console.WriteLine("Purging...");
            ValidateStatus(this.Purge(FTDI.FT_PURGE.FT_PURGE_TX | FTDI.FT_PURGE.FT_PURGE_RX));

            this.IsPortOpen = true;
        }

        public void PurgeForAxxess()
        {
            ValidateStatus(this.Purge(FTDI.FT_PURGE.FT_PURGE_TX | FTDI.FT_PURGE.FT_PURGE_RX));
        }
    }
}
