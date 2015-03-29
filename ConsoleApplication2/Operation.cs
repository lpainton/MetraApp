using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public OperationStatus Status { get; private set; }


    }
}
