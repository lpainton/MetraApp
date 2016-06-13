using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metra.Axxess;

namespace MetraWPFBrowserApp
{
    public delegate void DeviceRemovedHandler(object sender, EventArgs e);

    public static class BoardManager
    {
        public static IAxxessBoard ConnectedBoard
        {
            get
            {
                return _connectedBoard;
            }
            private set
            {
                _connectedBoard = value;
            }
        }
        private static IAxxessBoard _connectedBoard;

        public static void Initialize()
        {
            ConnectedBoard = null;
        }

        public static bool IsDeviceConnected
        {
            get { return ConnectedBoard != null; }
        }

        public static void SetConnectedBoard(IAxxessBoard dev)
        {
            if (ConnectedBoard != null)
                throw new InvalidOperationException();
            else
            {
                ConnectedBoard = dev;
                if (dev != null) dev.AddRemovedEvent(DeviceRemovedListener);
            }
        }

        public static void DeviceRemovedListener(object s, EventArgs e)
        {
            LogManager.WriteToLog("Device was removed.");
            ConnectedBoard.Dispose();
            ConnectedBoard = null;
            OnDeviceRemoved(s, e);
        }

        public static event DeviceRemovedHandler OnDeviceRemoved;

        public static bool IsBoardConnected { get { return _connectedBoard != null; } }
    }
}
