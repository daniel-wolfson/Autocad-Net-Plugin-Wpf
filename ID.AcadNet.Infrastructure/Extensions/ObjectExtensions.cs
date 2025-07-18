using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using applicationp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ID.AcadNet.Infrastructure.Extensions
{
    public static class ObjectExtensions
    {
        public static void AcadApplicationInvokeMember(string objectName, object[] args)
        {
            //object[] dataArry = new object[2];
            //dataArry[0] = false; //no save
            //dataArry[1] = ""; //drawing file name.. if saving

            //Dispatcher.CurrentDispatcher.Invoke(() =>
            //Commands.BackgroundDispatcher.BeginInvoke(new Action(() =>

            var acadObject = applicationp.AcadApplication;
            var activeDocument = acadObject.GetType()
                .InvokeMember("ActiveDocument", BindingFlags.GetProperty, null, acadObject, null);

            activeDocument.GetType()
                .InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, activeDocument, args);
        }
    }
}


//{
//    dynamic acad = Marshal.GetActiveObject("AutoCAD.Application");
//    dynamic activeDocument1 = acad.ActiveDocument;
//    activeDocument1.Editor.WriteMessage(activeDocument1.FullName);
//}
//catch (COMException ex)
//{
//    const uint MK_E_UNAVAILABLE = 0x800401e3;
//    if ((uint)ex.ErrorCode == MK_E_UNAVAILABLE)
//    {
//    }
//    else
//    {
//    }
//}
