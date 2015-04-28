using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Metra.Axxess
{
    class OperationFirmware : Operation
    {
        AxxessFirmware File { get; set; }
        IEnumerator<byte[]> _fileEnum;

        public OperationFirmware(IAxxessBoard device, AxxessFirmware file) : base(device, OperationType.Firmware)
        {
            this.File = file;
            this._fileEnum = this.File.GetEnumerator();
            this.TotalOperations = this.File.Count;
        }

        public override void DoWork()
        {
            this.Work();
        }

        public override void Work()
        {
            base.Work();

            //Register events
            this.Device.AddAckEvent(AckHandler);
            this.Device.AddFinalEvent(FinalHandler);

            //Send the ready packet and wait for reply
            this.Device.SendReadyPacket();
        }

        public void AckHandler(object sender, EventArgs e)
        {
            if (this.Status.Equals(OperationStatus.Working) && this._fileEnum.MoveNext())
            {
                this.Device.SendPacket(_fileEnum.Current);
                this.OperationsCompleted++;
            }
        }

        public void FinalHandler(object sender, EventArgs e)
        {
            this.OperationsCompleted++;
            this.Status = OperationStatus.Finished;
            this.Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();

            this.Device.RemoveAckEvent(AckHandler);
            this.Device.RemoveFinalEvent(FinalHandler);
        }
    }
}
