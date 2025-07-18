using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Intellidesk.AcadNet.Common.Internal
{
    /// <summary>
    /// The log manager.
    /// </summary>
    public static class LogManager
    {
        /// <summary>
        /// The log file name.
        /// </summary>
        public const string LogFile = "C:\\AcadCodePack.log";

        /// <summary>
        /// Writes object to logs.
        /// </summary>
        /// <param name="o">The object.</param>
        public static void Write(object o)
        {
            using (var sw = new StreamWriter(LogFile, append: true))
            {
                sw.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {o}");
            }
        }

        /// <summary>
        /// Writes object to logs.
        /// </summary>
        /// <param name="note">The note.</param>
        /// <param name="o">The object.</param>
        public static void Write(string note, object o)
        {
            using (var sw = new StreamWriter(LogFile, append: true))
            {
                sw.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {note}: {o}");
            }
        }

        /// <summary>
        /// Gets a time based name.
        /// </summary>
        /// <returns>The name.</returns>
        public static string GetTimeBasedName()
        {
            string timeString = DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToLongTimeString();
            return new string(timeString.Where(c => char.IsDigit(c)).ToArray());
        }

        /// <summary>
        /// The log table.
        /// </summary>
        public class LogTable
        {
            private readonly int[] _colWidths;

            /// <summary>
            /// The tab width.
            /// </summary>
            public const int TabWidth = 8;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="colWidths">The column widths. The values should be multiples of the tab width.</param>
            public LogTable(params int[] colWidths)
            {
                _colWidths = colWidths;
            }

            /// <summary>
            /// Gets the string representation of a row.
            /// </summary>
            /// <param name="contents">The contents.</param>
            /// <returns>The result.</returns>
            public string GetRow(params object[] contents)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < contents.Length; i++)
                {
                    string content = contents[i].ToString();
                    sb.Append(content);
                    int nTab = (int)Math.Ceiling((double)(_colWidths[i] - GetStringWidth(content)) / TabWidth);
                    for (int j = 0; j < nTab; j++)
                    {
                        sb.Append('\t');
                    }
                }
                return sb.ToString();
            }

            /// <summary>
            /// Gets string width: ASCII as 1, others as 2.
            /// </summary>
            /// <param name="content">The string.</param>
            /// <returns>The width.</returns>
            public static int GetStringWidth(string content)
            {
                return content.Sum(c => c > 255 ? 2 : 1);
            }
        }
    }
}
