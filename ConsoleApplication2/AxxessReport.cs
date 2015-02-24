using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    public class AxxessReport : Report
    {
        public AxxessReport(AxxessBoard dev) : base(dev) { }
    }

    public class TestInputReport : InputReport
    {
        public TestInputReport(AxxessBoard dev) : base(dev)
        {
        }

        public override void ProcessData()
        {
        }
    }

    public class TestOutputReport : OutputReport
    {
        public TestOutputReport(AxxessBoard dev) : base(dev) { }
    }

    public class IntroReport : OutputReport
    {
        public IntroReport(AxxessBoard dev) : base(dev)
        {
            this.SetBuffer(dev.IntroPacket);
        }
    }

    public class ReadyReport : OutputReport
    {
        public ReadyReport(AxxessBoard dev) : base(dev)
        {
            this.SetBuffer(dev.ReadyPacket);
        }
    }

    public class GenericReport : OutputReport
    {
        public GenericReport(AxxessBoard dev, byte[] packet) : base(dev)
        {
            this.SetBuffer(dev.PrepPacket(packet));
        }
    }
}
