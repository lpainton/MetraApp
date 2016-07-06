using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    /// <summary>
    /// To use a logging system with the library one only needs to implement the
    /// ILogger interface and then statically initialize the log with it.  All logging
    /// operations in the library will then write to it.
    /// </summary>
    public interface ILogger
    {
        void Write(string message);
    }

    /// <summary>
    /// Logmodes are used to limit the amount of information logged.
    /// </summary>
    public enum LogMode
    {
        Off = 0,
        Normal = 1,
        Verbose = 2,
    };

    /// <summary>
    /// The Log class controls access to a logger object provided to the library by an 
    /// implementing application.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// A singleton Logger object.  Ideally should be thread-safe since the Log object itself is.
        /// </summary>
        static ILogger Logger { get; set; }

        /// <summary>
        /// The logging mode, which can be set dynamically.
        /// </summary>
        public static LogMode Mode { get; set; }

        /// <summary>
        /// Allows an application to submit a logger object for use by the library.
        /// Locks any object already stored as the Logger to avoid race-conditions
        /// in the event a Logger is being replaced and written to synchronously.
        /// </summary>
        /// <param name="logger">The ILogger object to use.</param>
        /// <param name="mode">Optional mode specification.  Default is Normal.</param>
        public static void Initialize(ILogger logger, LogMode mode = LogMode.Normal)
        {
            if (Logger != null)
                lock (Logger)
                {
                    Logger = logger;
                }
            else
                Logger = logger;
            Mode = mode;
        }

        /// <summary>
        /// Thread-safe log write operation.  Introduces some overhead from the mutex.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="priority">
        /// The priority to write at.  
        /// Will not write to the log if the priority 
        /// doesn't match or supercede the current mode.</param>
        public static void Write(string message, LogMode priority = LogMode.Normal)
        {
            if (Logger != null && (Mode >= priority))
            {
                lock (Logger)
                {
                    Logger.Write(message);
                }
            }
        }
    }
}
