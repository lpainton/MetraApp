using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Metra.Axxess
{
    public class AxxessFirmwareInfo
    {
        public string ua { get; set; }
        public string url { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("UA:" + ua);
            sb.AppendLine("Url:" + url);
            return sb.ToString();
        }
    }

    public class AxxessFirmwareVersion : IComparable
    {
        protected int MajorVer { get; set; }
        protected int MinorVer { get; set; }

        public bool IsNull { get { return MajorVer == 0 && MinorVer == 0;} }

        public AxxessFirmwareVersion(string verString)
        {
            if (IsValidVersion(verString))
            {
                string major = verString.Substring(0,1);
                string minor = verString.Substring(1);
                
                MajorVer = Convert.ToInt32(major);
                MinorVer = Convert.ToInt32(minor);
            }
            else 
            {
                MajorVer = 0;
                MinorVer = 0;
            }
        }

        public bool IsValidVersion(string verString)
        {
            return (verString.Length > 1 && verString.Length < 4) ? 
                (!String.IsNullOrWhiteSpace(verString)) ? 
                !verString.Any(x => !Char.IsDigit(x)) ? true : false : false : false;
        }

        public override string ToString()
        {
            return MajorVer.ToString() + "." + MinorVer.ToString();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AxxessFirmwareVersion))
                throw new InvalidOperationException("Comparison of AxxessFirmwareVersion to non related object.");
            AxxessFirmwareVersion v = (AxxessFirmwareVersion)obj;
            return (v.MajorVer.Equals(this.MajorVer) && v.MinorVer.Equals(this.MinorVer));
        }

        public int CompareTo(object obj)
        {
            if (!(obj is AxxessFirmwareVersion))
                throw new InvalidOperationException("Comparison of AxxessFirmwareVersion to non related object.");
            AxxessFirmwareVersion vers = (AxxessFirmwareVersion)obj;

            int result = this.MajorVer.CompareTo(vers.MajorVer);
            return (result == 0) ? this.MinorVer.CompareTo(vers.MinorVer) : result;
        }

        int IComparable.CompareTo(object obj)
        {
            return this.CompareTo(obj);
        }
    }

    public class AxxessFirmwareToken
    {
        public string FileName { get; set; }
        public AxxessFirmwareVersion FileVersion { get; set; }
        public string BoardID { get; set; }

        public AxxessFirmwareToken(string name, string ver, string boardid)
        {
            FileName = name;
            FileVersion = new AxxessFirmwareVersion(ver);
            BoardID = boardid;
        }

        public bool IsNull { get { return FileName.Equals(String.Empty) && FileVersion.IsNull; } }

        public static AxxessFirmwareToken Null
        {
            get
            {
                return new AxxessFirmwareToken(String.Empty, String.Empty, String.Empty);
            }
        }
    }

    public class AxxessFirmware : IEnumerable<byte[]>
    {
        

        byte[] _hexFile;
        public int PacketSize { get; private set; }
        public int Count { get { return _hexFile.Length / this.PacketSize; } }
        public int _index;
        public int Index { get { return _index; } }
        
        AxxessFirmwareToken _token;
        public string Version { get { return this._token.FileVersion.ToString(); } }

        public AxxessFirmware(string path, int packetSize, AxxessFirmwareToken token)
        {
            this._token = token;
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
