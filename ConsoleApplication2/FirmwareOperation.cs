using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Metra.Axxess
{
    class FirmwareOperation : Operation
    {
        public string Path { get; private set; }
        public int Progress { get; private set; }
        public Firmware File { get; private set; }
        ManualResetEventSlim _MRE;

        public FirmwareOperation(IAxxessDevice device, string path) : base(device)
        {
            this.Path = path;
            this.Progress = 0;
            this.File = new Firmware(path, 44);
            this._MRE = new ManualResetEventSlim();

            //Register events
            //TODO: Include removal during disposal phase
            this.Device.AddAckEvent(AckHandler);
            this.Device.AddFinalEvent(FinalHandler);
        }

        public override void Work()
        {
            base.Work();
            _MRE.Reset();

            //Send the ready packet and wait for reply
            this.Device.SendReadyPacket();
            _MRE.Wait();

            foreach(byte[] packet in this.File)
            {
                if (!this.Status.Equals(OperationStatus.Working))
                    break;

                this.Device.SendPacket(packet);
                //this.Progress = TODO: Implement this!
                _MRE.Wait();
            }

            //TODO: Remove events here or call dispose
        }

        public void AckHandler(object sender, EventArgs e)
        {
            this._MRE.Set();
        }

        public void FinalHandler(object sender, EventArgs e)
        {
            this.Status.Equals(OperationStatus.Finished);
        }
    }
}
