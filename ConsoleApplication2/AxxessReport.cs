using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    public class IntroReport : OutputReport
    {
        public IntroReport(IAxxessBoard dev) : base((HIDDevice)dev)
        {
            this.SetBuffer(dev.IntroPacket);
        }
    }

    public class ReadyReport : OutputReport
    {
        public ReadyReport(IAxxessBoard dev) : base((HIDDevice)dev)
        {
            this.SetBuffer(dev.ReadyPacket);
        }
    }

    public class ASWCRequestReport : OutputReport
    {
        public ASWCRequestReport(IAxxessBoard dev) : base((HIDDevice)dev)
        {
            this.SetBuffer(dev.ASWCRequestPacket);
        }
    }

    public class GenericOutputReport : OutputReport
    {
        public GenericOutputReport(IAxxessBoard dev, byte[] packet) : base((HIDDevice)dev)
        {
            this.SetBuffer(dev.PrepPacket(packet));
        }
    }

    public class AxxessInputReport : InputReport
    {
        public AxxessInputReport(IAxxessBoard dev) : base((HIDDevice)dev)
        {
        }

        public override void ProcessData()
        {
            return;
        }
    }
}
