using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    public static class TLogger
    {
        private static string _FilePath;
        private static int _Hour = -1;
        public static string FilePath
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (_FilePath.IsNullOrEmpty() || _Hour != DateTime.Now.Hour)
                {
                    _FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", Date + ".txt");
                    _Hour = DateTime.Now.Hour;
                }
                return _FilePath;
            }
        }
        private static string Date
        {
            get
            {
                PersianCalendar persianCalendar = new PersianCalendar();
                return string.Format("{0:0000}-{1:00}-{2:00}", persianCalendar.GetYear(DateTime.Now), persianCalendar.GetMonth(DateTime.Now), persianCalendar.GetDayOfMonth(DateTime.Now));
            }
        }
        private static string DateAndTime
        {
            get
            {
                PersianCalendar persianCalendar = new PersianCalendar();
                DateTime DT = DateTime.Now;
                return string.Format("{0:0000}-{1:00}-{2:00} {3:00}-{4:00}-{5:00} ", persianCalendar.GetYear(DT), persianCalendar.GetMonth(DT), persianCalendar.GetDayOfMonth(DT), DT.Hour, DT.Minute, DT.Second);
            }
        }

        public static bool AllowLogToFile { get; set; } = true;

        public static void LogError(string message)
        {
            Trace.TraceError(DateAndTime + message);
            LogToFile(DateAndTime + message);
        }
        public static void LogError(Exception exc)
        {
            LogError(exc.ToString());
        }

        public static void LogInformation(string message)
        {
            message = DateAndTime + message;
            Trace.TraceInformation(message);
            LogToFile(message);
        }

        private static void LogToFile(string message)
        {
            if (!AllowLogToFile) return;
            try
            {
                FileInfo F = new FileInfo(FilePath);
                if (!F.Directory.Exists) F.Directory.Create();
                File.AppendAllText(FilePath, message + "\r\n", Encoding.UTF8);
            }
            catch
            {
            }
        }

        public static void LogWarning(string message)
        {
            message = DateAndTime + message;
            Trace.TraceWarning(message);
            LogToFile(message);
        }
        public static void Indent()
        {
            Trace.Indent();
        }
        public static void Unindent()
        {
            Trace.Unindent();
        }
        public static int IndentLevel
        {
            get
            {
                return Trace.IndentLevel;
            }
            set
            {
                Trace.IndentLevel = value;
            }
        }
        public static int IndentSize
        {
            get
            {
                return Trace.IndentSize;
            }
            set
            {
                Trace.IndentSize = value;
            }
        }
    }
}
