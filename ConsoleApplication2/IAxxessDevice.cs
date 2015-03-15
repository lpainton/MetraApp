using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    interface IAxxessDevice
    {
        /// <summary>
        /// Stores the numeric product ID for the device
        /// </summary>
        int ProductID { get; }

        /// <summary>
        /// Stores the application firmware version number
        /// </summary>
        int AppFirmwareVersion { get; }

        /// <summary>
        /// Stores the boot firmware version number
        /// </summary>
        int BootFirmwareVersion { get; }

        /// <summary>
        /// Forces idle status on the board.  Use when preparing for firmware installation.
        /// </summary>
        void StartForceIdle();
    }
}
