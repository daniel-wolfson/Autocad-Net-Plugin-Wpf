using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services
{
    /// <summary> BaseService </summary>
    public abstract class BaseService
    {
        /// <summary> current Db </summary>
        public static Database Db => HostApplicationServices.WorkingDatabase;

        /// <summary> current Doc </summary>
        public static Document Doc => Application.DocumentManager.MdiActiveDocument;

        public static IPluginSettings PluginSettings => Plugin.GetService<IPluginSettings>();

        public Matrix3d UCS => Doc.Editor.CurrentUserCoordinateSystem;
    }
}