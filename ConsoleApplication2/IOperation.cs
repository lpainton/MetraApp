using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    public interface IOperation
    {
        OperationStatus Status { get; }

        OperationType Type { get; }

        Thread WorkerThread { get; }

        int Progress { get; }

        void Start();

        void Stop();
    }
}
