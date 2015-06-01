using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    public abstract class OperationFactory
    {
        #region Static Methods
        public static IOperation SpawnOperation(OperationType type, OpArgs args)
        {
            switch (type)
            {
                case OperationType.Download:
                    return null;

                case OperationType.Boot:
                    return new OperationBoot(args.Device);

                case OperationType.Firmware:
                    return new OperationFirmware(args.Device, new AxxessFirmware(args.Path, args.Device.PacketSize));

                case OperationType.Remap:
                    return null;

                default:
                    return null;
            }
        }
        #endregion
    }
}
