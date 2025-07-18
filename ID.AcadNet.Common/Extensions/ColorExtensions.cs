using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using System;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Color = Autodesk.AutoCAD.Colors.Color;

namespace Intellidesk.AcadNet.Common.Extensions
{
    /// <summary> General </summary>
    public static class ColorExtensions
    {
        public static Editor Ed => Doc.Editor;
        public static Database Db => Doc.Database;
        public static Document Doc => App.DocumentManager.MdiActiveDocument;


        /// <summary> Common Color Dialog </summary>
        public static Color DlgColor
        {
            get
            {
                var cd = new ColorDialog { IncludeByBlockByLayer = true };
                cd.ShowModal();
                return cd.Color;
            }
        }

        public static System.Windows.Media.Color ToMediaColor(this Color acadColor)
        {
            return System.Windows.Media.Color.FromRgb(acadColor.Red, acadColor.Green, acadColor.Blue);
        }
        public static string ToHtmlColor(this Color acadColor)
        {
            System.Drawing.Color color = acadColor.ColorValue;
            //string hex = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            return System.Drawing.ColorTranslator.ToHtml(color);
        }
        private static string ToHexString(this Color acadColor)
        {
            System.Drawing.Color color = acadColor.ColorValue;
            return String.Format("#{0:X6}", color.ToArgb() & 0x00FFFFFF);
        }
        public static string ToRgbString(this Color acadColor)
        {
            System.Drawing.Color color = acadColor.ColorValue;
            return String.Format("RGB({0},{1},{2})", color.R, color.G, color.B);
        }
    }
}