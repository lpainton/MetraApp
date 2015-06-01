using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Metra.Axxess
{
    class OperationBoot : Operation
    {
        const int SLEEP_TIME = 100;
        //Cycles to allow before timing out for non-responsive board.
        const int TIMEOUT_MAX = 50;

        public OperationBoot(IAxxessBoard device) : base(device, OperationType.Boot)
        {
            this.TotalOperations = 1;
            this.OperationsCompleted = 0;

            Device.AddIntroEvent(IntroHandler);

            this.Timeout = TIMEOUT_MAX;
            this.CanTimeOut = true;
        }

        public override void Work()
        {
            base.Work();

            try
            {
                this.Device.SendIntroPacket();
                Thread.Sleep(SLEEP_TIME);
            }
            catch (Exception e)
            {
                this.Stop();
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            Device.RemoveIntroEvent(IntroHandler);
        }

        public void IntroHandler(object sender, EventArgs e)
        {
            if (!Device.ProductID.Equals(String.Empty))
            {
                OperationsCompleted = 1;
                Timeout = 0;
                ResetTimeout();
                CanTimeOut = false;
            }
        }
    }
}
