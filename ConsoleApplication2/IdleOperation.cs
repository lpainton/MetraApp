using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    class IdleOperation : Operation
    {
        public IdleOperation(IAxxessDevice device) : base(device)
        {

        }

        public override void Work()
        {
            base.Work();

            Device.SendIntroPacket();
        }

        public void Stop()
        {
            this.Status = OperationStatus.Finished;
        }
    }
}
