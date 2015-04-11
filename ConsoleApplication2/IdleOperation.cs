using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Metra.Axxess
{
    class IdleOperation : Operation
    {
        const int SLEEP_TIME = 100;

        public IdleOperation(IAxxessDevice device) : base(device)
        {

        }

        public override void Work()
        {
            base.Work();

            this.Device.SendIntroPacket();
            Thread.Sleep(SLEEP_TIME);
        }
    }
}
