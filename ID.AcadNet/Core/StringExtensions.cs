using System;

namespace Intellidesk.AcadNet.Core
{
    public static class StringExtensions
    {
        const string Newline = "\r\n";

        public static string InsertNewlineAtLastSpace(this string s)
        {
            // If the string contains a space, replace it
            // with a newline character, otherwise we simply
            // append the newline to the string

            string ret;
            if (s.Contains(" "))
            {
                int index = s.LastIndexOf(" ");
                ret =
                    s.Substring(0, index) + Newline +
                    s.Substring(index + 1);
            }
            else
            {
                ret = s + Newline;
            }
            return ret;
        }

        public static string ReplaceNewlinesWithSpaces(this string s)
        {
            // Replace all the newlines with spaces
            if (String.IsNullOrEmpty(s))
                return s;
            return s.Replace(Newline, " ");
        }

        public static string GetLastLine(this string s)
        {
            // Return the last line of the text (or
            // the whole thing if there's no newline in it)
            var ret = s.Contains(Newline) ? s.Substring(s.LastIndexOf(Newline) + Newline.Length) : s;
            return ret;
        }

        public static int GetLineCount(this string s)
        {
            // Count the number of lines by checking the
            // overall length of the string against the
            // string without newline sequences in it
            return ((s.Length - s.Replace(Newline, "").Length) / Newline.Length) + 1;
        }
    }
}