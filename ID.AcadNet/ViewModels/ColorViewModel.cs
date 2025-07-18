using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.Data.Models.Entities;
using System;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.ViewModels
{
    public class ColorViewModel : BaseEntity
    {
        bool _showingColorDlg;
        private AcadColor _currentColor;

        private AcadColors _colors;
        public AcadColors Colors
        {
            get { return _colors; }
            set
            {
                _colors = value;
                OnPropertyChanged();
            }
        }

        public ColorViewModel(Type type, short colorKey)
        {
            //DataInfoAttribute dataInfoAttr = (DataInfoAttribute)Attribute.GetCustomAttribute(type, typeof(DataInfoAttribute));

            Colors = new AcadColors();
            var attrs = type.GetAttributes<DataInfoAttribute>();
            attrs.ForEach(att => Colors.Add(new AcadColor((short)att.Value.ColorIndex)));

            CurrentColor = Colors.ElementAtOrDefault(colorKey) != null
                ? Colors[colorKey]
                : Colors.FirstOrDefault(x => x.ColorIndex == -1);
            IsReadOnly = true;
        }

        public AcadColor CurrentColor
        {
            get { return _currentColor; }
            set
            {
                if (value != null && !Equals(_currentColor, value))
                {
                    if (!_showingColorDlg && value.ColorIndex == -3)
                    {
                        _showingColorDlg = true;
                        _currentColor.ColorIndex = ShowColorDlg();
                    }
                    else
                    {
                        _currentColor = value;
                    }

                    _showingColorDlg = false;
                }
                OnPropertyChanged();
            }
        }

        public short ShowColorDlg()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            Autodesk.AutoCAD.Windows.ColorDialog dlg = new Autodesk.AutoCAD.Windows.ColorDialog();
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return 255;
            }

            Color clr = dlg.Color;
            if (!clr.IsByAci)
            {
                if (clr.IsByLayer)
                {
                    return Color.FromColorIndex(ColorMethod.ByLayer, 255).ColorIndex; //"ByLayer";
                }

                if (clr.IsByBlock)
                {
                    return Color.FromColorIndex(ColorMethod.ByBlock, 255).ColorIndex; //"ByBlock";
                }

                if (!Colors.Select(x => x.ColorIndex).Contains(clr.ColorIndex))
                    Colors.Insert(Colors.Count - 1, new AcadColor(clr.ColorIndex));

                return clr.ColorIndex;
            }

            //Byte byt = Convert.ToByte(clr.ColorIndex);
            //int rgb = EntityColor.LookUpRgb(byt);
            //long b = rgb & 0xffL;
            //long g = (rgb & 0xff00L) >> 8;
            //long r = rgb >> 16;

            if (!Colors.Select(x => x.ColorIndex).Contains(clr.ColorIndex))
                Colors.Insert(Colors.Count - 1, new AcadColor(clr.ColorIndex));

            return clr.ColorIndex;
        }

        public void Reload()
        {
            //Colors = AcadNetManager.Colors.Cast<T>().ToList().Cast<AcadColor>().ToList();
        }
    }
}