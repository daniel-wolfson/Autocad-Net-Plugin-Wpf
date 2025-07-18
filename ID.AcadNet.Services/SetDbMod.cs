using System;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services
{
    public class SetDbMod
    {
        #region acdbSetDbmod

        [DllImport("acdb17.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbSetDbmod@@YAJPAVAcDbDatabase@@J@Z")]
        private static extern Int32 acdbSetDbmod17x86(IntPtr db, Int32 newDbMod);

        [DllImport("acdb17.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbSetDbmod@@YAJPEAVAcDbDatabase@@J@Z")]
        private static extern Int32 acdbSetDbmod17x64(IntPtr db, Int32 newDbMod);

        [DllImport("acdb18.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbSetDbmod@@YAJPAVAcDbDatabase@@J@Z")]
        private static extern Int32 acdbSetDbmod18x86(IntPtr db, Int32 newDbMod);

        [DllImport("acdb18.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbSetDbmod@@YAJPEAVAcDbDatabase@@J@Z")]
        private static extern Int32 acdbSetDbmod18x64(IntPtr db, Int32 newDbMod);

        [DllImport("acdb19.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbSetDbmod@@YAJPAVAcDbDatabase@@J@Z")]
        private static extern Int32 acdbSetDbmod19x86(IntPtr db, Int32 newDbMod);

        [DllImport("acdb19.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbSetDbmod@@YAJPEAVAcDbDatabase@@J@Z")]
        private static extern Int32 acdbSetDbmod19x64(IntPtr db, Int32 newDbMod);

        [DllImport("acdb20.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbSetDbmod@@YAJPAVAcDbDatabase@@J@Z")]
        private static extern Int32 acdbSetDbmod20x86(IntPtr db, Int32 newDbMod);

        [DllImport("acdb20.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbSetDbmod@@YAJPEAVAcDbDatabase@@J@Z")]
        private static extern Int32 acdbSetDbmod20x64(IntPtr db, Int32 newDbMod);

        [DllImport("acdb21.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbSetDbmod@@YAJPAVAcDbDatabase@@J@Z")]
        private static extern Int32 acdbSetDbmod21x86(IntPtr db, Int32 newDbMod);

        [DllImport("acdb21.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbSetDbmod@@YAJPEAVAcDbDatabase@@J@Z")]
        private static extern Int32 acdbSetDbmod21x64(IntPtr db, Int32 newDbMod);

        public static Int32 acdbSetDbmod(ref Database db, Int32 newDbMod)
        {
            if (IntPtr.Size == 4)
                switch (Application.Version.Major)
                {
                    case 17: return acdbSetDbmod17x86(db.UnmanagedObject, newDbMod);
                    case 18: return acdbSetDbmod18x86(db.UnmanagedObject, newDbMod);
                    case 19: return acdbSetDbmod19x86(db.UnmanagedObject, newDbMod);
                    case 20: return acdbSetDbmod20x86(db.UnmanagedObject, newDbMod);
                    case 21: return acdbSetDbmod21x86(db.UnmanagedObject, newDbMod);
                    default: return (Int32)ErrorStatus.NotImplementedYet;
                }
            else
                switch (Application.Version.Major)
                {
                    case 17: return acdbSetDbmod17x64(db.UnmanagedObject, newDbMod);
                    case 18: return acdbSetDbmod18x64(db.UnmanagedObject, newDbMod);
                    case 19: return acdbSetDbmod19x64(db.UnmanagedObject, newDbMod);
                    case 20: return acdbSetDbmod20x64(db.UnmanagedObject, newDbMod);
                    case 21: return acdbSetDbmod21x64(db.UnmanagedObject, newDbMod);
                    default: return (Int32)ErrorStatus.NotImplementedYet;
                }
        }
        #endregion

        // Установка DBMOD для активного документа
        [CommandMethod("SetCurDbMod")]
        public static void SetCurDbMod()
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            acdbSetDbmod(ref db, 1);
        }

        // Очистка DBMOD для активного документа
        [CommandMethod("ClearCurDbMod")]
        public static void ClearCurDbMod()
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            acdbSetDbmod(ref db, 0);
        }

        // Очистка DBMOD для всех открытых документов
        [CommandMethod("ClearAllDbMod")]
        public static void ClearAllDbMod()
        {
            foreach (Document doc in Application.DocumentManager)
            {
                Database db = doc.Database;
                acdbSetDbmod(ref db, 0);
            }
        }
    }
}