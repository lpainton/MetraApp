using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    class FirmwareOperation : Operation
    {
        public string Path { get; private set; }

        public FirmwareOperation(IAxxessDevice device, string path) : base(device)
        {
            this.Path = path;
        }

        public override void Work()
        {
            base.Work();


        }
    }
}
