using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface ICommandBase
    {
        Database Db { get; }
        Document Doc { get; }
        Editor Ed { get; }
        void Dispose();
    }
}