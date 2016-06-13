using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    public interface ILogger
    {
        void Write(string message);
    }

    public enum LogMode
    {
        Off = 0,
        Normal = 1,
        Verbose = 2,
    };

    public static class Log
    {
        static ILogger Logger { get; set; }
        public static LogMode Mode { get; set; }

        public static void Initialize(ILogger logger, LogMode mode = LogMode.Normal)
        {
            Logger = logger;
            Mode = mode;
        }

        public static void Write(string message, LogMode priority = LogMode.Normal)
        {
            if (Logger != null && (Mode >= priority))
            {
                Logger.Write(message);
            }
        }
    }
}
