using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Metra.Axxess
{
    class Firmware : IEnumerable<byte[]>
    {
        byte[] _hexFile;
        public int PacketSize { get; private set; }
        public int Count { get { return _hexFile.Length / this.PacketSize; } }

        public Firmware(string path, int packetSize)
        {
            this._hexFile = File.ReadAllBytes(path);
            this.PacketSize = packetSize;
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            for (int i = 0; i < (_hexFile.Length / this.PacketSize); i++)
            {
                byte[] packet = new byte[this.PacketSize];
                for (int j = 0; j < this.PacketSize; j++)                
                    packet[j] = this._hexFile[((i * this.PacketSize) + j)];
                yield return packet;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
