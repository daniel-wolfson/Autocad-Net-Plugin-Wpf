using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Extensions;
using Unity;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services
{
    /// <summary> Service manager </summary>
    public static class ServiceManager
    {
        public static IUnityContainer Container { get; private set; }

        static ServiceManager()
        {
            var appSettings = Plugin.GetService<IPluginSettings>();
            acadApp.DocumentManager.MdiActiveDocument.AddRegAppTableRecord(appSettings.Name);
        }

        public static T GetService<T>() where T : class
        {
            return Container.Resolve<T>();
        }
    }
}
