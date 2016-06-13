using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Metra.Axxess;

namespace MetraWPFBrowserApp
{
    public enum LoggingMode
    {
        Active,
        Passive,
        None
    };

    public class AppLog : Queue<string>, ILogger
    {
        public AppLog() : base()
        {
        }

        public void Write(string s) { this.Enqueue(s); }

        void ILogger.Write(string message)
        {
            this.Write(message);
        }
    }

    public static class LogManager
    {
        const int QUEUE_SIZE = 20;
        const string LOG_FILE_EXTENSION = ".log";

        public static LoggingMode Mode { get; set; }
        static string FileName { get; set; }

        public static AppLog Log { get; private set; }

        public static void Initialize(LoggingMode mode, FileManager fman)
        {
            Mode = mode;
            LogManager.Log = new AppLog();
            DateTime d = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.Append(d.Year); 
            sb.AppendFormat("{0:2}", d.Month.ToString("D2")); 
            sb.AppendFormat("{0:2}", d.Day.ToString("D2"));
            sb.AppendFormat("{0:2}", d.Hour.ToString("D2"));
            sb.AppendFormat("{0:2}", d.Minute.ToString("D2"));
            sb.AppendFormat("{0:2}", d.Second.ToString("D2"));  
            sb.Append(LOG_FILE_EXTENSION);
            FileName = Path.Combine(fman.LogsFolder, sb.ToString());
            fman.CreateDirectory(fman.LogsFolder);
            File.Create(FileName);
            
            WriteToLog("Logging session started on " + DateTime.Now.ToString());
            WriteToLog("Axxess App for Windows");

            System.Reflection.Assembly asm = typeof(LogManager).Assembly;
            System.Reflection.AssemblyName name = asm.GetName();
            WriteToLog("Version: " + name.Version.ToString());
        }

        public static void CloseOut()
        {
            FlushLog();
        }

        public static void WriteToLog(string s)
        {
            if (Mode == LoggingMode.Active) Console.WriteLine(s);
            LogManager.Log.Write(DateTime.Now.TimeOfDay.ToString() + ": " + s);
            if (LogManager.Log.Count > QUEUE_SIZE)
                FlushLog();
        }

        public static void FlushLog()
        {
            FlushLog(FileName);
        }

        public static void FlushLog(string file)
        {
            if (LogManager.Log.Count > 0)
            {
                try
                {
                    if (!(Mode == LoggingMode.None)) 
                        File.AppendAllLines(file, LogManager.Log.ToArray());
                    LogManager.Log.Clear();
                }
                catch 
                {
                    Console.WriteLine("Unable to flush log!");
                }
            }
        }
    }
}
