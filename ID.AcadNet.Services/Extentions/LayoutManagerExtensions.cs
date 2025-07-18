using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Services.Enums;
using System;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services.Extentions
{
    /// <summary> Autodesk.AutoCAD.DatabaseServices.LayoutManager Extentions </summary>
    public static class LayoutManagerExtensions
    {
        /// <summary>
        /// This is Extension Method for the <c>Autodesk.AutoCAD.DatabaseServices.LayoutManager</c> 
        /// class. It gets the current space in the current Database.
        /// </summary>
        /// <param name="mng">Target <c>Autodesk.AutoCAD.DatabaseServices.LayoutManager</c> 
        /// instance.</param>
        /// <returns>Returns the SpaceEnum value.</returns>        
        public static eSpaceEnum GetCurrentSpaceEnum(this LayoutManager mng)
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Int16 tilemode = (Int16)Application.GetSystemVariable("TILEMODE");

            if (tilemode == 1)
                return eSpaceEnum.Model;

            Int16 cvport = (Int16)Application.GetSystemVariable("CVPORT");
            if (cvport == 1)
                return eSpaceEnum.Layout;
            else
                return eSpaceEnum.Viewport;
        }

        /// <summary>
        /// This is Extension Method for the <c>Autodesk.AutoCAD.DatabaseServices.LayoutManager</c> 
        /// class. It gets the name of the current space in the current Database.
        /// </summary>
        /// <param name="mng">Target <c>Autodesk.AutoCAD.DatabaseServices.LayoutManager</c> 
        /// instance.</param>
        /// <returns>Returns the name of current space.</returns>
        public static string GetCurrentSpaceName(this LayoutManager mng)
        {
            eSpaceEnum space = GetCurrentSpaceEnum(mng);
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            String modelSpaceLocalizedName = String.Empty;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForRead) as BlockTableRecord;
                modelSpaceLocalizedName = (tr.GetObject(btr.LayoutId, OpenMode.ForRead) as Layout).LayoutName;
            }
            String result = space == eSpaceEnum.Viewport ? "Model" as String : mng.CurrentLayout;
            return result;
        }

        /// <summary>
        /// This is Extension Method for the <c>Autodesk.AutoCAD.DatabaseServices.LayoutManager</c> 
        /// class. It gets the localized name of the Model tab.
        /// </summary>
        /// <param name="mng">Target <c>Autodesk.AutoCAD.DatabaseServices.LayoutManager</c> 
        /// instance.</param>
        /// <returns>Returns the name of current space.</returns>
        public static String GetModelTabLocalizedName(this LayoutManager mng)
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            String modelTabLocalizedName = String.Empty;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForRead) as BlockTableRecord;
                modelTabLocalizedName = (tr.GetObject(btr.LayoutId, OpenMode.ForRead) as Layout).LayoutName;
            }
            return modelTabLocalizedName;
        }
    }
}