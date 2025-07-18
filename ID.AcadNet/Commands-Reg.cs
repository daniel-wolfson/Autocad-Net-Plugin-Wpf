using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure.Commands;
using Intellidesk.AcadNet;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Services;
using Microsoft.Win32;
using System.Reflection;
using Registry = Microsoft.Win32.Registry;
using RegistryKey = Microsoft.Win32.RegistryKey;

[assembly: CommandClass(typeof(CommandReg))]
namespace Intellidesk.AcadNet
{
    public class CommandReg : CommandLineBase
    {
        [CommandMethod(CommandNames.MainGroup, CommandNames.Reg, CommandFlags.Session | CommandFlags.NoHistory)]
        public void Register()
        {
            //if (!Database.Exists("AcadNetContext"))
            //    Database.SetInitializer(new DropCreateDatabaseIfModelChanges<AcadNetContext>());

            if (RegisterService.RegisterForDemandLoading(Assembly.GetExecutingAssembly()))
                Ed.WriteMessage(CommandNames.UserGroup + ": AcadNet plugin has been successfuly registered");

            //Doc.SendStringToExecute(CommandNames.Ribbon,true,false,false);
        }

        /// <summary> UnRegistration plugin </summary>
        [CommandMethod(CommandNames.MainGroup, CommandNames.UnReg, CommandFlags.Session | CommandFlags.NoHistory)]
        public void Unregister()
        {
            if (RegisterService.UnregisterForDemandLoading(Assembly.GetExecutingAssembly()))
                Ed.WriteMessage(CommandNames.UserGroup + ": AcadNet plugin has been successfuly unregistered");
        }

        public void RegisterPlugin()
        {
            //get key AutoCAD from register
            string sProdKey = HostApplicationServices.Current.MachineRegistryProductRootKey;
            string sAppName = PluginSettings.Name;

            RegistryKey regAcadProdKey = Registry.CurrentUser.OpenSubKey(sProdKey);
            RegistryKey regAcadAppKey = regAcadProdKey.OpenSubKey("Applications", true);

            //if app exist
            string[] subKeys = regAcadAppKey.GetSubKeyNames();
            foreach (string subKey in subKeys)
            {
                //if app registered - exit
                if (subKey.Equals(sAppName))
                {
                    regAcadAppKey.Close();
                    return;
                }
            }

            string sAssemblyPath = Assembly.GetExecutingAssembly().Location;

            // reg app
            RegistryKey regAppAddInKey = regAcadAppKey.CreateSubKey(sAppName);
            regAppAddInKey.SetValue("DESCRIPTION", sAppName, RegistryValueKind.String);
            regAppAddInKey.SetValue("LOADCTRLS", 14, RegistryValueKind.DWord);
            regAppAddInKey.SetValue("LOADER", sAssemblyPath, RegistryValueKind.String);
            regAppAddInKey.SetValue("MANAGED", 1, RegistryValueKind.DWord);

            regAcadAppKey.Close();
            //CommandLine.SendToExecute(CommandNames.Ribbon);
        }

        public void UnregisterPlugin()
        {
            //get key from register
            string sProdKey = HostApplicationServices.Current.MachineRegistryProductRootKey;
            string sAppName = PluginSettings.Name;

            RegistryKey regAcadProdKey = Registry.CurrentUser.OpenSubKey(sProdKey);
            RegistryKey regAcadAppKey = regAcadProdKey.OpenSubKey("Applications", true);

            //delete key
            regAcadAppKey.DeleteSubKeyTree(sAppName);
            regAcadAppKey.Close();
        }
    }
}

