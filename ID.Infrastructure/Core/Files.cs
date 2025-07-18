using ID.Infrastructure.General;
using IWshRuntimeLibrary;
using Newtonsoft.Json;
using Shell32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using File = System.IO.File;
using Folder = Shell32.Folder;

namespace ID.Infrastructure
{
    public static class Files
    {
        public static string GetFileName(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;

            var sections = fullPath.Split('\\');
            return sections[sections.Length - 1];
        }

        public static string GetExt(string fullPath)
        {
            return fullPath.Substring(fullPath.LastIndexOf(".") + 1);
        }

        public static string GetBaseFolderName(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;

            var sections = fullPath.Split('\\');
            return sections.Length >= 1 ? sections[1] : "";
        }

        public static string GetBaseRootFolderName(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;

            var sections = fullPath.Split('\\');
            return sections.Length >= 2 ? sections[2] : "";
        }

        public static string GetPathWitoutDrive(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;

            var sections = fullPath.Split('\\').ToList();
            sections.RemoveAt(0);
            return Path.Combine(sections.ToArray());
        }

        public static void DirectoryScan()
        {
            var list = new ConcurrentBag<string>();
            string[] dirNames = { ".", ".." };
            var tasks = new List<Task>();
            foreach (var dirName in dirNames)
            {
                string name = dirName;
                Task t = Task.Run(() =>
                {
                    foreach (var path in Directory.GetFiles(name))
                        list.Add(path);
                });
                tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray());
            foreach (Task t in tasks)
                Console.WriteLine("Task {0} Status: {1}", t.Id, t.Status);

            Console.WriteLine("Number of files read: {0}", list.Count);
        }

        /// <summary> Validate folder name </summary>
        public static bool IsValidFilename(string testName)
        {
            string chars = Regex.Escape(string.Concat(Path.GetInvalidFileNameChars())).Replace("*", "")
                .Replace("?", "");
            Regex containsABadCharacter = new Regex("[" + chars + "]");
            return !containsABadCharacter.IsMatch(testName);
        }

        /// <summary> Validate folder name </summary>
        public static bool IsValidPathname(string testName)
        {
            string chars = Regex.Escape(string.Concat(Path.GetInvalidPathChars())).Replace("*", "").Replace("?", "");
            Regex containsABadCharacter = new Regex("[" + chars + "]");
            return !containsABadCharacter.IsMatch(testName);
        }

        public static string Normal(string str)
        {
            return new string(str.Replace("[", "").Replace("]", "").Replace(" ", "").Replace("?", "a").ToCharArray());
        }

        public static SimpleActionResult LoadJsonFileData<T>(string fullPath) where T : class
        {
            //JObject o1 = JObject.Parse(File.ReadAllText(@"c:\videogames.json"));
            //JsonTextReader reader = new JsonTextReader(file)

            const Int32 BufferSize = 128;

            try
            {
                using (var fileStream = File.OpenRead(fullPath))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    string[] lines = streamReader.ReadToEnd()
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    if (lines.Length < 2) throw new InvalidDataException("Must have header line.");

                    string[] headers = Normal(lines[1]).Split(',');
                    //StringBuilder sb = new StringBuilder();
                    List<string> jsonResult = new List<string>();

                    for (int i = 2; i < lines.Length; i++)
                    {
                        string[] fields = Normal(lines[i]).Split(',');

                        if (fields.Length == headers.Length)
                        {
                            //throw new InvalidDataException("Field count must match header count.");

                            var jsonElements = headers.Zip(fields, (header, field) => $"{header}: {field}").ToArray();
                            string jsonObject = "{" + $"{string.Join(",", jsonElements)}" + "}";

                            if (i < lines.Length - 1)
                            {
                                //jsonObject += ",";
                                jsonResult.Add(jsonObject.Replace(",:", "")); //JObject.Parse
                            }
                            //sb.AppendLine(jsonObject);
                        }
                    }

                    //JObject o1 = JObject.Parse(File.ReadAllText(fullPath));
                    //using (StreamReader file = File.OpenText(@"c:\videogames.json"))
                    //using (JsonTextReader reader = new JsonTextReader(file))
                    //{
                    //    JObject o2 = (JObject)JToken.ReadFrom(reader);
                    //}

                    //var arr = JArray.Parse($"[{sb}]".Replace(",: ", "").Replace("{{", "{").Replace("}}", "}".Replace(",]", "]")));

                    var arr = $"[{string.Join(",", jsonResult)}]";

                    List<T> elements = JsonConvert.DeserializeObject<List<T>>(arr);

                    //foreach (var variable in variables)
                    //{
                    //    LatLng latLng = WebMercator.itm2gps(
                    //        new Point(variable.Latitude.GetValueOrDefault(), variable.Longitude.GetValueOrDefault()));
                    //    variable.CoordX = Math.Round(latLng.lat, 2); ;
                    //    variable.CoordY = Math.Round(latLng.lng, 2);
                    //}
                    //var result = objects.Select(JsonConvert.SerializeObject).ToArray();

                    return new SimpleActionResult
                    {
                        Message = fullPath,
                        StatusCode = HttpStatusCode.Found,
                        ActionResult = elements
                    }; //(JObject)JToken.ReadFrom(reader)
                }
            }
            catch (Exception ex)
            {
                return new SimpleActionResult { Message = ex, StatusCode = HttpStatusCode.NotFound, ActionResult = null };
            }
        }

        //public static SimpleActionResult FindPath(string fullPath, string fileName)
        //{
        //    IPluginSettings pluginSettings = PluginSettings.Load(CommandNames.UserGroup);
        //    return FindPath(pluginSettings.IncludeFolders.ToArray(), fullPath, fileName);
        //}

        public static SimpleActionResult FindPath(string[] paths, string searchPattern, string fileName = null,
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!string.IsNullOrEmpty(fileName))
                fileName = fileName.Contains("dwg") ? fileName : fileName + ".dwg";

            HttpStatusCode code = HttpStatusCode.Found;
            var errMessage = "";

            if (string.IsNullOrEmpty(searchPattern))
            {
                code = HttpStatusCode.InternalServerError;
                errMessage = "Command 'FindFile': fullPath are both empty.";
                return new SimpleActionResult() { Message = errMessage, StatusCode = code, ActionResult = searchPattern };
            }

            bool found = false;

            //if (paths == null)
            //{
            //    IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
            //    paths = pluginSettings.IncludeFolders.ToArray();
            //}

            foreach (var path in paths)
            {
                string[] folders = Directory.GetDirectories(path, Path.GetFileName(searchPattern), searchOption);
                if (folders.Any())
                {
                    searchPattern = folders[0];
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                code = HttpStatusCode.NotFound;
                errMessage = string.Format("Command 'FindPath': {0} not found.", searchPattern);
                return new SimpleActionResult() { Message = errMessage, StatusCode = code, ActionResult = searchPattern };
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                if (!File.Exists(searchPattern + "\\" + fileName))
                {
                    string[] files = Directory.GetFiles(searchPattern, fileName, searchOption);
                    if (files.Length > 0)
                        searchPattern = files[0];
                    else
                    {
                        code = HttpStatusCode.InternalServerError;
                        errMessage = string.Format("Command 'FindPath': {0} not found.", searchPattern);
                    }
                }
                else
                {
                    searchPattern = searchPattern + "\\" + fileName;
                }
            }
            return new SimpleActionResult() { Message = errMessage, StatusCode = code, ActionResult = searchPattern };
        }

        public static void CreateShortcutTargetFile(string targetFullPath)
        {
            try
            {
                var fileName = GetFileName(targetFullPath);
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var path = Path.Combine(appDataPath, "Intellidesk", "WorkItems", $"{fileName}.lnk");
                //{GetBaseRootFolderName(targetFullPath)}{GetBaseFolderName(targetFullPath)}_

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(path);
                shortcut.TargetPath = targetFullPath;
                //shortcut.IconLocation = 'Location of  iCon you want to set";

                // add Description of Short cut
                shortcut.Description = "Intellix shortcut";

                // save it / create
                shortcut.Save();
            }
            catch (Exception) { }
        }

        public static string GetShortcutTargetFile(string shortcutFilename)
        {
            try
            {
                string pathOnly = Path.GetDirectoryName(shortcutFilename);
                string filenameOnly = Path.GetFileName(shortcutFilename);

                Shell shell = new Shell();
                Folder folder = shell.NameSpace(pathOnly);
                FolderItem folderItem = folder.ParseName(filenameOnly);
                if (folderItem != null)
                {
                    ShellLinkObject link = (ShellLinkObject)folderItem.GetLink;
                    return link.Path;
                }
            }
            catch (Exception) { }

            return string.Empty;
        }

        //public static string GetShortcutTargetFile1(string shortcutFilename)
        //{
        //    string pathOnly = Path.GetDirectoryName(shortcutFilename);
        //    string filenameOnly = Path.GetFileName(shortcutFilename);

        //    Shell32.Shell shell = new Shell32.ShellClass();
        //    Shell32.Folder folder = shell.NameSpace(pathOnly);
        //    Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
        //    if (folderItem != null)
        //    {
        //        Shell32.ShellLinkObject link =
        //            (Shell32.ShellLinkObject)folderItem.GetLink;
        //        return link.Path;
        //    }
        //    return String.Empty; // Not found
        //}

        public static void CreateShortcut2(string linkName, string location)
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Intellidesk", "WorkItems", linkName + ".lnk");
            using (StreamWriter writer = new StreamWriter(path))
            {
                //string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=file:///" + location);
                writer.WriteLine("IconIndex=0");
                string icon = location.Replace('\\', '/');
                writer.WriteLine("IconFile=" + icon);
                writer.Flush();
            }
        }


    }
}
