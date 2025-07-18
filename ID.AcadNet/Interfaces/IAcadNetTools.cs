using Autodesk.AutoCAD.DatabaseServices;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Services.Core;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface IAcadNetTools
    {
        void OnParseObjectsClick(object sender, UIControlEventArgs args);
        void OnToolsParseObjects(ObjectId sender, ActionArguments args);
        void OnToolsParseBlock(ObjectId sender, ActionArguments args);
    }
}