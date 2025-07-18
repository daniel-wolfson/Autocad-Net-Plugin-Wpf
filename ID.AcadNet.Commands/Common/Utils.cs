using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Intellidesk.AcadNet.Commands.Common
{
    using IniData = Dictionary<string, Dictionary<string, string>>;

    /// <summary>
    /// Utilities.
    /// </summary>
    public static class Utils
    {
        #region File algorithms

        /// <summary>
        /// Gets the path relative to a base path.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="path">The other path.</param>
        /// <returns>The relative path.</returns>
        /// <example>
        /// string strPath = GetRelativePath(@"C:\WINDOWS\system32", @"C:\WINDOWS\system\*.*" );
        /// //strPath == @"..\system\*.*"
        /// </example>
        public static string GetRelativePath(string basePath, string path)
        {
            if (!basePath.EndsWith("\\")) basePath += "\\";
            int intIndex = -1, intPos = basePath.IndexOf('\\');
            while (intPos >= 0)
            {
                intPos++;
                if (string.Compare(basePath, 0, path, 0, intPos, true) != 0) break;
                intIndex = intPos;
                intPos = basePath.IndexOf('\\', intPos);
            }

            if (intIndex >= 0)
            {
                path = path.Substring(intIndex);
                intPos = basePath.IndexOf("\\", intIndex);
                while (intPos >= 0)
                {
                    path = "..\\" + path;
                    intPos = basePath.IndexOf("\\", intPos + 1);
                }
            }
            return path;
        }

        /// <summary>
        /// Parses INI files
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="result">The result.</param>
        /// <returns>A value indicating if suceeded.</returns>
        public static bool ParseIniFile(string fileName, IniData result)
        {
            var groupPattern = @"^\[[^\[\]]+\]$";
            var dataPattern = @"^[^=]+=[^=]+$";

            var lines = File.ReadAllLines(fileName)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line))
                .ToArray();

            var group = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.StartsWith("["))
                {
                    if (!Regex.IsMatch(line, groupPattern))
                    {
                        return false;
                    }
                    group = new Dictionary<string, string>();
                    var groupName = line.Trim('[', ']');
                    result.Add(groupName, group);
                }
                else
                {
                    if (!Regex.IsMatch(line, dataPattern))
                    {
                        return false;
                    }
                    var parts = line.Split('=').Select(part => part.Trim()).ToArray();
                    group.Add(parts[0], parts[1]);
                }
            }
            return true;
        }

        #endregion
    }
}
