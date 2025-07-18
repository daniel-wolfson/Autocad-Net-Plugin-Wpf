using System;

namespace Intellidesk.Common
{
    public static class LogHelper
    {
        public static void Debug(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        public static void Debug(string s, Exception ex)
        {
            Debug($"{s},exception:{ex.Message}");
        }

        public static void Debug(Exception ex)
        {
            Debug($"exception:{ex.Message}");
        }

        public static void DebugFormat(string format, params object[] values)
        {
            Debug(string.Format(format, values));
        }

        public static void Info(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }
    }
}
