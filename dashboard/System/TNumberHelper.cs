using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class TNumberHelper
    {
        public static string SecondsToString(this int seconds)
        {
            //string.Format("{0:00}:{1:00}:{2:00}", objPath.TotalVideoDuration / 3600, (objPath.TotalVideoDuration / 60) % 60, objPath.TotalVideoDuration % 60)
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return string.Format("{0}:{1:00}:{2:00}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }
    }
}
