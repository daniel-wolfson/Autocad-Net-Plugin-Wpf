using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface ICommandLine : ICommandBase
    {
        void Alert(string alert);
        void AppInters();
        void Cancel();
        void Enter();
        void Error(string commanName, string message);
        void Execute(string commandName, ITaskArguments commandArgs);
        string Current();
        //IDictionary<string, ITaskArguments> CommandInProcessList { get; set; }
        ResultBuffer InvokeLisp(ResultBuffer args);
        void LoadCuixMenu(ref Assembly assem, string sectionName = "");
        void Regen();
        void ReloadLinetype();
        void SendToExecute(string cmd, string cmdArg = "", int tCmdEcho = 0, int tFileDia = 0, bool tActivate = true, bool tWrapUpInactiveDoc = false, bool tEchoCommand = false);
        void ViewIsometric(int iJob);
        void Viewport(Vector3d vec);
        void WriteMessage(string s);
        void Zoom();
        void Zoom(Point3d[] points);
        void Zoom(ObjectIdCollection objIds, double dFactor = 1);
        void Zoom(ObjectId objectId, double dFactor = 1);
        void Zoom(List<Entity> obj, double dFactor = 1);
        void Zoom(Entity ent, double dFactor = 1);
        void Zoom(double xmin, double ymin, double xmax, double ymax);
        void ZoomWindow(Point3d pntMin, Point3d pntMax, double dFactor = 1);
        void ZoomView(Point3d pntMin, Point3d pntMax, Point3d? pntCenter, double dFactor = 1);
        void ZoomLimits();
        void ZoomExtents();
        void ZoomExtents(Database db);
        void UpdateScreen();
        Task<bool> PageExists(string url);
    }
}