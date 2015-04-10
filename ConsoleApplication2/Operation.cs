using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Metra.Axxess
{
    enum OperationStatus
    {
        Ready,
        Working,
        Finished
    };

    class Operation
    {
        public OperationStatus Status { get; protected set; }

        public Thread WorkerThread { get; private set; }
        public ThreadStart WorkerMethod { get; private set; }

        public IAxxessDevice Device { get; private set; }

        public Operation(IAxxessDevice device)
        {
            this.WorkerMethod = this.DoWork;
            this.WorkerThread = new Thread(this.WorkerMethod);

            this.Device = device;

            this.Status = OperationStatus.Ready;
        }

        public virtual void DoWork()
        {
            while(this.Status.Equals(OperationStatus.Working))
            {
                this.Worker();
            }
        }

        public virtual void Work()
        {
            return;
        }

        public virtual void Start()
        {
            this.Status = OperationStatus.Working;
            this.WorkerThread.Start();
        }

        public virtual void Stop()
        {
            this.Status = OperationStatus.Finished;
            this.WorkerThread.Abort();
        }
    }
}
