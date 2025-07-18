using System.Text;
using System.Web.Http;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services.WebApi
{
    /// <summary> AcadActionController </summary>
    public class AcadActionController : ApiController 
    {
        private static StringBuilder _msg = new StringBuilder();
        public Document Doc { get { return Application.DocumentManager.MdiActiveDocument; } }
        public Editor Ed { get { return Doc.Editor; } }
        public Database Db { get { return Doc.Database; } }
        public static StringBuilder ActionMessage
        {
            get { return _msg; }
        }
    }    
}
