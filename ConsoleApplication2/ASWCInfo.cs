using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    public class ASWCButtonMap
    {

    }

    public class ASWCInfo
    {
        public int VersionMajor { get; private set; }
        public int VersionMinor { get; private set; }
        public int VersionBuild { get; private set; }
        public int RadioType { get; private set; }
        public int CarCommBus { get; private set; }
        public int CarCommMethod { get; private set; }
        public int CarFlavor { get; private set; }
        public bool StalkPresent { get; private set; }
        public int StalkOrientation { get; private set; }


        public ASWCInfo(byte[] raw)
        {
        }

        private void ProcessRawPacket(byte[] raw)
        {

        }
    }
}
