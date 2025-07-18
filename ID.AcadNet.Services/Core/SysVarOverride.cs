using System;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services.Core
{
    /// <summary> SysVarOverride </summary>
    public class SysVarOverride : IDisposable
    {
        readonly object _oldValue;
        readonly string _varName;

        /// <summary> ctor </summary>
        public SysVarOverride(string name, object value)
        {
            _varName = name;
            _oldValue = App.GetSystemVariable(name);
            App.SetSystemVariable(name, value);
            App.SetSystemVariable(name, value);
        }

        public void Dispose()
        {
            App.SetSystemVariable(_varName, _oldValue);
        }
    }
}
