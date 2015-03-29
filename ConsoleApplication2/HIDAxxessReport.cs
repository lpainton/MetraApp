﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    /*public class HIDAxxessReport : Report
    {
        public HIDAxxessReport(HIDAxxessBoard dev) : base(dev) { }
    }*/

    public class IntroReport : OutputReport
    {
        public IntroReport(IAxxessDevice dev) : base((HIDDevice)dev)
        {
            this.SetBuffer(dev.IntroPacket);
        }
    }

    public class ReadyReport : OutputReport
    {
        public ReadyReport(IAxxessDevice dev) : base((HIDDevice)dev)
        {
            this.SetBuffer(dev.ReadyPacket);
        }
    }

    public class GenericReport : OutputReport
    {
        public GenericReport(IAxxessDevice dev, byte[] packet) : base((HIDDevice)dev)
        {
            this.SetBuffer(dev.PrepPacket(packet));
        }
    }
}
