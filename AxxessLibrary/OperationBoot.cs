using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Metra.Axxess
{
    /// <summary>
    /// A boot operation puts and keeps and Axxess board in boot-mode, where it is receptive to
    /// firmware update operations.  It is a necessary precursor to the firmware operation.
    /// </summary>
    class OperationBoot : Operation
    {
        const int SLEEP_TIME = 100;
        //Cycles to allow before timing out for non-responsive board.
        const int TIMEOUT_MAX = 50;

        internal OperationBoot(IAxxessBoard device) : base(device, OperationType.Boot)
        {
            this.TotalOperations = 1;
            this.OperationsCompleted = 0;

            Device.AddIntroEvent(IntroHandler);

            this.Timeout = TIMEOUT_MAX;
            this.CanTimeOut = true;
        }

        /// <summary>
        /// Method send an intro packet then sleeps the thread to give time for a reply.
        /// This is done repeatedly unless the operation is stopped or times out.
        /// </summary>
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
        
        /// <summary>
        /// Called when the attached device receives an intro packet.
        /// Every time this fires we know the board is still in boot mode.
        /// </summary>
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
