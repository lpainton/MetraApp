using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Metra.Axxess
{
    public class AxxessFirmware : IEnumerable<byte[]>
    {
        byte[] _hexFile;
        public int PacketSize { get; private set; }
        public int Count { get { return _hexFile.Length / this.PacketSize; } }
        public int _index;
        public int Index { get { return _index; } }

        public AxxessFirmware(string path, int packetSize)
        {
            this._hexFile = File.ReadAllBytes(path);
            this.PacketSize = packetSize;
            this._index = -1;
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            for (int i = 0; i < (_hexFile.Length / this.PacketSize); i++)
            {
                byte[] packet = new byte[this.PacketSize];
                for (int j = 0; j < this.PacketSize; j++)                
                    packet[j] = this._hexFile[((i * this.PacketSize) + j)];
                this._index++;
                yield return packet;
            }
            this._index = -1;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
