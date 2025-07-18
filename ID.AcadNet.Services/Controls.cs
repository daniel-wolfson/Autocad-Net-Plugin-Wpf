using System.Threading;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;

namespace Intellidesk.AcadNet.Services
{
    public static class Controls
    {
        [CommandMethod("IntelliDesk", "WAITWINDOW", CommandFlags.Session | CommandFlags.NoHistory)]
        public static void WaitWindow()
        {
            var _manualResetEvent = new ManualResetEvent(false);

            var _waitWindowThread = new Thread(() =>
            {
                while (ComponentManager.Ribbon != null && ComponentManager.Ribbon.IsVisible)
                {
                    _manualResetEvent.Set();
                }
            });
            _waitWindowThread.SetApartmentState(ApartmentState.STA);
            _waitWindowThread.IsBackground = true;
            _waitWindowThread.Name = "Plugin wait ribbon";
            _waitWindowThread.Start();

            _manualResetEvent.WaitOne();
            _waitWindowThread.Abort();
            _waitWindowThread.Join();

            //var waitWindow = UIServiceCenter.CreateWaitWindow();
            //waitWindow.AddMessage("Initializing");
            //waitWindow.AddMessage("Loading");
            //waitWindow.AddMessage("Done!", 3000);
            //waitWindow.Complete();
        }
    }
}