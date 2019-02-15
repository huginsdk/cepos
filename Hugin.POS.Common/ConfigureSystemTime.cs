using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Hugin.POS.Common
{
    public struct SYSTEMTIME
    {
        private ushort wYear;
        private ushort wMonth;
        private ushort wDayOfWeek;
        private ushort wDay;
        private ushort wHour;
        private ushort wMinute;
        private ushort wSecond;
        private ushort wMilliseconds;

        /// <summary>
        /// Convert form System.DateTime
        /// </summary>
        /// <param name="time"></param>
        public void SetDateTime(DateTime time)
        {
            wYear = (ushort)time.Year;
            wMonth = (ushort)time.Month;
            wDayOfWeek = (ushort)time.DayOfWeek;
            wDay = (ushort)time.Day;
            wHour = (ushort)time.Hour;
            wMinute = (ushort)time.Minute;
            wSecond = (ushort)time.Second;
            wMilliseconds = (ushort)time.Millisecond;            
        }
    }
  public  class ConfigureSystemTime
    {
#if WindowsCE
        [DllImport("coredll.dll", SetLastError = true)]
#else
        [DllImport("Kernel32.dll", SetLastError = true)]
#endif
        public static extern bool SetLocalTime(ref SYSTEMTIME Time);

        [DllImport("Kernel32.dll")]
        public static extern uint GetLastError();


    }
}
