using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using Microsoft.Win32;
using System;
using Serilog;

namespace Intellidesk.AcadNet.Services
{
    public class RegisterService
    {
        public static string GetKey(Assembly assem)
        {
            var baseKey = Registry.CurrentUser;
            var regKey = baseKey.OpenSubKey("Software\\Autodesk\\AutoCAD", false);
            if (regKey != null)
                regKey = baseKey.OpenSubKey(regKey.Name.Substring(baseKey.Name.Length + 1) + "\\" + regKey.GetValue("CurVer", ""), false);
            if (regKey != null)
                regKey = baseKey.OpenSubKey(regKey.Name.Substring(baseKey.Name.Length + 1) + "\\" + regKey.GetValue("CurVer", "") + "\\Applications\\");
            if (regKey != null) return regKey.Name.Substring(Registry.CurrentUser.Name.Length + 1);
            return "";
        }

        //tStartUp ? 2 : 12
        public static bool RegisterForDemandLoading(Assembly assem, bool currentUser = true, bool startup = true, bool force = true)
        {
            // Get the assembly, its name and location
            var name = assem.GetName().Name;
            var path = assem.Location;

            // We'll collect information on the commands
            // (we could have used a map or a more complex
            // container for the global and localized names
            // - the assumption is we will have an equal
            // number of each with possibly fewer groups)

            var globCmds = new List<string>();
            var locCmds = new List<string>();
            var groups = new List<string>();

            // Iterate through the modules in the assembly
            var mods = assem.GetModules(true);
            foreach (var mod in mods)
            {
                // Within each module, iterate through the types
                var types = mod.GetTypes();
                foreach (var type in types)
                {
                    // We may need to get a type's resources
                    var rm = new ResourceManager(type.FullName, assem) {IgnoreCase = true};

                    // Get each method on a type
                    var meths = type.GetMethods();
                    foreach (var meth in meths)
                    {
                        // Get the method's custom attribute(s)
                        var attbs = CustomAttributeData.GetCustomAttributes(meth);
                        foreach (var attb in attbs)
                        {
                            // We only care about our specific attribute type
                            if (attb.Constructor.DeclaringType.Name == "CommandMethodAttribute")
                            {
                                // Harvest the information about each command
                                string globName;
                                string locName;

                                // Our processing will depend on the number of
                                // parameters passed into the constructor
                                int paramCount = attb.ConstructorArguments.Count;
                                if (paramCount == 1 || paramCount == 2)
                                {
                                    // Constructor options here are:
                                    //  globName (1 argument)
                                    //  grpName, globName (2 args)
                                    globName = attb.ConstructorArguments[0].ToString();
                                    locName = globName;
                                }
                                else if (paramCount >= 3)
                                {
                                    // Constructor options here are:
                                    //  grpName, globName, flags (3 args)
                                    //  grpName, globName, locNameId, flags (4 args)
                                    //  grpName, globName, locNameId, flags, hlpTopic (5 args)
                                    //  grpName, globName, locNameId, flags, contextMenuType (5 args)
                                    //  grpName, globName, locNameId, flags, contextMenuType, hlpFile, helpTpic (7 args)
                                    CustomAttributeTypedArgument arg0, arg1;
                                    arg0 = attb.ConstructorArguments[0];
                                    arg1 = attb.ConstructorArguments[1];

                                    // All options start with grpName, globName
                                    var grpName = arg0.Value as string;
                                    globName = arg1.Value as string;
                                    locName = globName;

                                    // If we have a localized command ID,
                                    // let's look it up in our resources
                                    if (paramCount >= 4)
                                    {
                                        // Get the localized string ID
                                        var lid = attb.ConstructorArguments[2].ToString();

                                        // Strip off the enclosing quotation marks
                                        if (lid.Length > 2)
                                            lid = lid.Substring(1, lid.Length - 2);

                                        // Let's put a try-catch block around this
                                        // Failure just means we use the global
                                        // name twice (the default)

                                        if (lid != "")
                                        {
                                            try
                                            {
                                                locName = lid; // rm.GetString(lid);
                                            }
                                            catch { }
                                        }
                                    }

                                    if (globName != null)
                                    {
                                        // Add the information to our data structures

                                        globCmds.Add(globName);
                                        locCmds.Add(locName);

                                        if (grpName != null && !groups.Contains(grpName))
                                            groups.Add(grpName);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Let's register the application to load on AutoCAD
            // startup (2) if specified or if it contains no
            // commands. Otherwise we will have it load on
            // command invocation (12)
            int flags = (startup && globCmds.Count > 0 ? 2 : 12);

            // Now create our Registry keys
            return CreateDemandLoadingEntries(
              name, path, globCmds, locCmds, groups,
              flags, currentUser, force
            );
        }

        public static bool UnregisterForDemandLoading(Assembly assem)
        {
            // Get the name of the application to unregister
            var appName = assem.GetName().Name;

            // Unregister it for both HKCU and HKLM
            bool res = RemoveDemandLoadingEntries(appName, true);
            res &= RemoveDemandLoadingEntries(appName, false);

            // If one call failed, we also fail (could change this)
            return res;
        }

        // Helper functions
        private static bool CreateDemandLoadingEntries(string appName, string path,
          IList<string> globCmds, IList<string> locCmds, IList<string> groups,
          int flags, bool currentUser, bool force)
        {
            var acKeyName = GetAutoCADKey();
            var hive = (currentUser ? Registry.CurrentUser : Registry.LocalMachine);

            // We may need to create the Applications key, as some AutoCAD
            // verticals do not contain it under HKCU by default
            // CreateSubKey just opens existing keys for write, anyway
            var appk = hive.CreateSubKey(acKeyName + "\\" + "Applications");
            using (appk)
            {
                // Already registered? Just return (unless forcing)
                if (!force)
                {
                    var subKeys = appk.GetSubKeyNames();
                    if (subKeys.Any(subKey => subKey.Equals(appName)))
                    {
                        return false;
                    }
                }

                // Create the our application's root key and its values
                var rk = appk.CreateSubKey(appName);
                using (rk)
                {
                    rk.SetValue("DESCRIPTION", appName, RegistryValueKind.String);
                    rk.SetValue("LOADCTRLS", flags, RegistryValueKind.DWord);
                    rk.SetValue("LOADER", path, RegistryValueKind.String);
                    rk.SetValue("MANAGED", 1, RegistryValueKind.DWord);

                    // Create a subkey if there are any commands...
                    if ((globCmds.Count == locCmds.Count) &&
                        globCmds.Count > 0)
                    {
                        var ck = rk.CreateSubKey("Commands");
                        using (ck)
                        {
                            for (int i = 0; i < globCmds.Count; i++)
                                ck.SetValue(globCmds[i], locCmds[i], RegistryValueKind.String);
                        }
                    }

                    // And the command groups, if there are any

                    if (groups.Count > 0)
                    {
                        var gk = rk.CreateSubKey("Groups");
                        using (gk)
                        {
                            foreach (var grpName in groups)
                                gk.SetValue(grpName, grpName, RegistryValueKind.String);
                        }
                    }
                }
            }
            return true;
        }

        private static bool RemoveDemandLoadingEntries(string appName, bool currentUser)
        {
            try
            {
                var ackName = GetAutoCADKey();

                // Choose a Registry hive based on the function input
                var hive = (currentUser ? Registry.CurrentUser : Registry.LocalMachine);

                // Open the applications key
                var appk = hive.OpenSubKey(ackName + "\\" + "Applications", true);
                using (appk)
                {
                    // Delete the key with the same name as this assembly
                    appk.DeleteSubKeyTree(appName);
                }
            }
            catch(Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                return false;
            }
            return true;
        }

        /// <summary> GetAutoCADKey </summary>
        /// <returns>version of autocad plugin</returns>
        public static string GetAutoCADKey()
        {
            var retValue = "";
            // Start by getting the CurrentUser location
            var hive = Registry.CurrentUser;

            // Open the main AutoCAD key
            var ack = hive.OpenSubKey("Software\\Autodesk\\AutoCAD");
            using (ack)
            {
                // Get the current major version and its key
                var ver = ack.GetValue("CurVer") as string;
                if (ver != null)
                {
                    var verk = ack.OpenSubKey(ver);
                    using (verk)
                    {
                        // Get the vertical/language version and its key
                        var lng = verk.GetValue("CurVer") as string;
                        if (lng != null)
                        {
                            var lngk = verk.OpenSubKey(lng);
                            using (lngk)
                            {
                                // And finally return the path to the key,
                                // without the hive prefix

                                retValue = lngk.Name.Substring(hive.Name.Length + 1);
                            }
                        }
                    }
                }
            }
            return retValue;
        }
    }
}

