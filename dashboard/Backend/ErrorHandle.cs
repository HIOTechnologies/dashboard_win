using System;
using System.Diagnostics;
using System.IO;

namespace HIO.Backend
{
    class ErrorHandle
    {
        private static readonly object _lock = new object();
        public void logEvent(string log)
        {
            try
            {
                lock (_lock)
                {
                    using (var file = new StreamWriter(Path.GetTempPath() + "\\logEvent_HIO.log", true))
                    {
                        file.WriteLine(DateTime.Now + "   " + log);
                        file.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }

        }
        public void ErrorFunc(Exception ex)
        {
            try
            {
                lock (_lock)
                {
                    StackTrace st = new StackTrace(ex, true);
                    //Get the first stack frame
                    StackFrame frame = st.GetFrame(0);

                    //Get the file name
                    string fileName = frame.GetFileName();

                    //Get the method name
                    string methodName = frame.GetMethod().Name;

                    //Get the line number from the stack frame
                    int line = frame.GetFileLineNumber();

                    //Get the column number
                    int col = frame.GetFileColumnNumber();

                    using (var file = new StreamWriter(Path.GetTempPath() + "\\log_HIO.log", true))
                    {
                        file.WriteLine(DateTime.Now + "   " + fileName + "   " + methodName + "      " + ex.Message + line + col);
                        file.Close();
                    }
                }
            }
            catch (Exception exc)
            {

            }
        }
        public void ErrorFunc(string err)
        {
            lock (_lock)
            {
                using (var file = new StreamWriter(Path.GetTempPath() + "\\log_HIO.log", true))
                {
                    file.WriteLine(DateTime.Now + " Error: " + err);
                    file.Close();
                }
            }
        }
    }
}
