using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FTD2XX_NET;

namespace Metra.Axxess
{
    /// <summary>
    /// An exception which contains the FTDI device status related to the exception.
    /// </summary>
    public class FTDIException : Exception
    {
        public FTDIException(FTDI.FT_STATUS status) : base(status.ToString()) { }
    }

    /// <summary>
    /// The result of a read operation on the FTDI device.
    /// </summary>
    public class FTDIAsynchReadResult : IAsyncResult
    {
        /// <summary>
        /// The state container for the read operation.
        /// </summary>
        public object State { get; set; }
        /// <summary>
        /// Did the operation complete?
        /// </summary>
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

    /// <summary>
    /// Class representation of the FTDI cable device used to communicate with FTDI boards.
    /// </summary>
    /// <remarks>
    /// The Axxess FTDI board consists of two parts.  A special FTDI cable which is its own USB device and
    /// the board it connects to which reads and writes the FTDI protocols.  This class represents the 
    /// cable portion and the AxxessFTDIBoard is the board portion.
    /// 
    /// This class uses the FTD2XX_NET wrapper provided by FTDI.
    /// </remarks>
    public class FTDICable : FTDI
    {
        public bool IsPortOpen { get; private set; }

        public FTDICable() : base()
        {
            this.IsPortOpen = false;
        }

        /// <summary>
        /// This method polls the connected device to find the baud rate it responds to.
        /// </summary>
        /// <param name="rates">An array of allowed rates.</param>
        /// <param name="timeout">The time in ms to allow for a response before moving on.</param>
        /// <returns>The found baud rate.</returns>
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

        /// <summary>
        /// Opens the FTDI port using presets appropriate for Axxess devices.
        /// </summary>
        /// <param name="baudRate">
        /// The baud rate to open at.  
        /// Currently Axxess devices only use 19200 or 115200.
        /// </param>
        public void OpenPortForAxxess(uint baudRate)
        {
            OpenCommPort(baudRate, FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
        }

        /// <summary>
        /// Validates that the status of the FTDI connection is OK.
        /// Throws an exception if not.
        /// </summary>
        /// <param name="status">A submitted FTDI status.</param>
        private void ValidateStatus(FTDI.FT_STATUS status)
        {
            if (status != FTDI.FT_STATUS.FT_OK)
                throw new FTDIException(status);
        }

        /// <summary>
        /// Writes a variable length packet of bytes to the device.
        /// </summary>
        /// <param name="packet">The packet to write.</param>
        /// <returns>Number of bytes written.</returns>
        public uint WriteToPort(byte[] packet)
        {
            UInt32 numBytesWritten = 0;
            this.Write(packet, packet.Length, ref numBytesWritten);
            return numBytesWritten;
        }

        /// <summary>
        /// Reads a set number of bytes from the device.
        /// </summary>
        /// <param name="buffer">Reference to the buffer which will hold the bytes read.</param>
        /// <param name="numBytes">The number of bytes to read.</param>
        /// <returns>The number of bytes actually read.</returns>
        public uint ReadFromPort(byte[] buffer, uint numBytes = 44)
        {
            UInt32 numBytesRead = 0;
            this.Read(buffer, numBytes, ref numBytesRead);
            return numBytesRead;
        }

        /// <summary>
        /// Beings an asynch read thread.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="callback">The callback used at the end of the read.</param>
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

        /// <summary>
        /// Closes the port.
        /// </summary>
        public void CloseCommPort() 
        { 
            ValidateStatus(this.Close());
            this.IsPortOpen = false;
        }

        /// <summary>
        /// Opens the port using the provided parameters.  Validates status us OK at each step.
        /// </summary>
        /// <param name="baudRate">The baud rate to open at.</param>
        /// <param name="wordLen">The length of each word in bytes.</param>
        /// <param name="stopBits">Any stop bits.</param>
        /// <param name="parity">Parity bytes.</param>
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

        /// <summary>
        /// Clears the transmission buffers.
        /// </summary>
        public void PurgeForAxxess()
        {
            ValidateStatus(this.Purge(FTDI.FT_PURGE.FT_PURGE_TX | FTDI.FT_PURGE.FT_PURGE_RX));
        }
    }
}
