using System;

namespace Metra.Axxess
{
	/// <summary>
	/// Base class for report types. Simply wraps a byte buffer.
	/// </summary>
    /// <remarks>
    /// From: Ashley Deakin's HIDLib
    /// http://www.developerfusion.com/article/84338/making-usb-c-friendly/
    /// </remarks>
	public abstract class Report
	{
		#region Member variables
		/// <summary>Buffer for raw report bytes</summary>
		private byte[] m_arrBuffer;
		/// <summary>Length of the report</summary>
		private int m_nLength;
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="oDev">Constructing device</param>
		public Report(HIDDevice oDev)
		{
			// Do nothing
		}

		/// <summary>
		/// Sets the raw byte array.
		/// </summary>
		/// <param name="arrBytes">Raw report bytes</param>
		protected void SetBuffer(byte[] arrBytes)
		{
			m_arrBuffer = arrBytes;
			m_nLength = m_arrBuffer.Length;
		}

        protected void ClearBuffer()
        {
            m_arrBuffer = new byte[0];
            m_nLength = 0;
        }

		/// <summary>
		/// Accessor for the raw byte buffer
		/// </summary>
		public byte[] Buffer
		{
			get
			{
				return m_arrBuffer;
			}
		}
		/// <summary>
		/// Accessor for the buffer length
		/// </summary>
		public int BufferLength
		{
			get
			{
				return m_nLength;
			}
		}

        public static void PrintReport(Report r)
        {
            byte[] packet = r.Buffer;
            PrintBuffer(packet);
        }

        public static void PrintBuffer(byte[] packet)
        {
            if (packet.Length > 0)
            {
                foreach (byte b in packet)
                    Console.Write("{0:x2}, ", b);
                Console.WriteLine();
            }
        }

        public static void EnumerateBuffer(byte[] packet)        
        {
            for (int i = 0; i < packet.Length; i++)
            {
                Console.WriteLine("{0}) {1:x2}", i, packet[i]);
            }
        }

        public static void CharacterizeBuffer(byte[] packet)
        {
            for (int i = 0; i < packet.Length; i++)
            {
                Console.WriteLine("{0}) {1:x2} {2}", i, packet[i], Convert.ToChar(packet[i]));
            }
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < BufferLength; i++)
            {
                sb.AppendFormat("{1:x2} ", i, Buffer[i]);
            }
            return sb.ToString();
        }
	}
	/// <summary>
	/// Defines a base class for output reports. To use output reports, just put the bytes into the raw buffer.
	/// </summary>
	public abstract class OutputReport : Report
	{
		/// <summary>
		/// Construction. Setup the buffer with the correct output report length dictated by the device
		/// </summary>
		/// <param name="oDev">Creating device</param>
		public OutputReport(HIDDevice oDev) : base(oDev)
		{
			SetBuffer(new byte[oDev.OutputReportLength]);
		}

        /*public OutputReport()
        {
            // TODO: Complete member initialization
        }*/
	}
	/// <summary>
	/// Defines a base class for input reports. To use input reports, use the SetData method and override the 
	/// ProcessData method.
	/// </summary>
	public abstract class InputReport : Report
	{
		/// <summary>
		/// Construction. Do nothing
		/// </summary>
		/// <param name="oDev">Creating device</param>
		public InputReport(HIDDevice oDev) : base(oDev)
		{
		}
		/// <summary>
		/// Call this to set the buffer given a raw input report. Calls an overridable method to
		/// should automatically parse the bytes into meaningul structures.
		/// </summary>
		/// <param name="arrData">Raw input report.</param>
		public void SetData(byte[] arrData)
		{
			SetBuffer(arrData);
			ProcessData();
		}
		/// <summary>
		/// Override this to process the input report into something useful
		/// </summary>
		public abstract void ProcessData();
	}
}
