using System.Collections.Generic;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.Infrastructure.Interfaces;

namespace Intellidesk.AcadNet.Services.Interfaces
{
    public interface ICommandLineService
    {
        void Cancel();
        void Enter();
        void SendToExecute(string cmd, ITaskArguments args);

        void SendToExecute(string cmd, string cmdArg = "", int tCmdEcho = 0, int tFileDia = 0,
            bool tActivate = true, bool tWrapUpInactiveDoc = false, bool tEchoCommand = false);

        void ReloadLinetype();

        ResultBuffer InvokeLisp(ResultBuffer args);

        void Regen();
        void WriteMessage(string s);
        void Alert(string alert);
        void LoadCuixMenu(ref Assembly assem, string sectionName = "");
        void Zoom();
        void Zoom(Point3d[] points);
        void Zoom(double tXmin, double tYmin, double tXmax, double tYmax);
        void Zoom(List<Entity> tObj);
        void Zoom(ObjectId objectId);
        void Zoom(ObjectIdCollection tObjIds);
        void ViewIsometric(int iJob);
        void Viewport(Vector3d vec);
        void UpdateScreen();

        void ExecuteOpenDwg(string filename, string searchPath);
    }
}