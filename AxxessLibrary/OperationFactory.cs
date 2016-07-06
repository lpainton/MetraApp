using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    /// <summary>
    /// Abstract factory which provides static methods for spawning operations.
    /// Use this to spawn valid operations with well defined parameters.
    /// </summary>
    /// <remarks>
    /// This was originally intended to cover all operations from firmware to ASWC,
    /// however the ASWC handling was offloaded to the boards since its protocol
    /// was much simpler overall.
    /// </remarks>
    public abstract class OperationFactory
    {
        #region Static Methods
        /// <summary>
        /// Spawns an operation of the given type.
        /// Spawning a firmware operation requires an OpArgs object with a Path, PacketSize and FileToken.
        /// </summary>
        /// <param name="type">The type of op to spawn.</param>
        /// <param name="args">An opargs object with the required information to spawn</param>
        /// <returns></returns>
        public static IOperation SpawnOperation(OperationType type, OpArgs args)
        {
            switch (type)
            {
                case OperationType.Boot:
                    return new OperationBoot(args.Device);

                case OperationType.Firmware:
                    return new OperationFirmware(args.Device, new AxxessFirmware(args.Path, args.Device.PacketSize, args.Token));

                default:
                    return null;
            }
        }
        #endregion
    }
}
