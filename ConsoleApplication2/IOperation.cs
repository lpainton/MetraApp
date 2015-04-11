using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    interface IOperation
    {
        OperationStatus Status { get; }

        int Progress { get; }

        void Start();

        void Stop();
    }
}
