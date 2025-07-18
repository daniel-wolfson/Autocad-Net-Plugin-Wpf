using System;
using System.Reflection;

namespace Intellidesk.AcadNet.Services.Extentions
{
    public static class LateBinding
    {
        public static object GetInstance(string appName)
        {
            return System.Runtime.InteropServices.Marshal.GetActiveObject(appName);
        }

        public static object CreateInstance(string appName)
        {
            return System.Activator.CreateInstance(System.Type.GetTypeFromProgID(appName));
        }

        public static object GetOrCreateInstance(string appName)
        {
            try { return GetInstance(appName); }
            catch { return CreateInstance(appName); }
        }


        public static object Get(this object obj, string propName, params object[] parameter)
        {
            return obj.GetType().InvokeMember(propName, BindingFlags.GetProperty, null, obj, parameter);
        }

        public static void Set(this object obj, string propName, params object[] parameter)
        {
            obj.GetType().InvokeMember(propName, BindingFlags.SetProperty, null, obj, parameter);
        }

        public static T Set<T>(this T element, Func<T, T> fn) where T : ICloneable
        {
            return fn((T)element.Clone());
        }

        //public static object Invoke(this object obj, string methName, params object[] parameter)
        //{
        //    return obj.GetType().InvokeMember(methName, BindingFlags.InvokeMethod, null, obj, parameter);
        //}

        public static void ReleaseInstance(this object obj)
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
        }
    }
}