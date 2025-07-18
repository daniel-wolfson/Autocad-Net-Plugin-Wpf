using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure.Enums;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Models;
using System;
using System.ComponentModel;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.ViewModels
{
    public class ClosurePanelElementContext : BaseViewModel
    {
        #region <props>

        private AcadClosures _closureTypes;
        private AcadClosure _currentClosure;
        private AcadClosure _closureType;

        public AcadClosures ClosureTypes
        {
            get { return _closureTypes; }
            set
            {
                _closureTypes = value;
                OnPropertyChanged();
            }
        }

        public AcadClosure ClosureType
        {
            get { return _closureType; }
            set
            {
                _closureType = value;
                OnPropertyChanged();
            }
        }

        public AcadClosure CurrentElement
        {
            get { return _currentClosure; }
            set
            {
                _currentClosure.PropertyChanged -= OnCurrentElementChanged;
                _currentClosure = value;
                _currentClosure.PropertyChanged += OnCurrentElementChanged;
                OnPropertyChanged();
            }
        }

        public bool IsAddTitleEnabled =>
            !string.IsNullOrEmpty(CurrentElement.Title) &&
            !string.IsNullOrEmpty(CurrentElement.Handle) &&
            CurrentElement.Items.Length > 0;

        #endregion

        #region ctor
        public ClosurePanelElementContext(eClosureType cableType)
        {
            _closureTypes = new AcadClosures();
            _closureType = _closureTypes.FirstOrDefault(x => x.TypeCode == cableType);
            _currentClosure = new AcadClosure(cableType);
        }
        #endregion

        #region <methods>
        public string ShowClosureDlg()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            Autodesk.AutoCAD.Windows.ColorDialog dlg = new Autodesk.AutoCAD.Windows.ColorDialog();
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return string.Empty;

            Autodesk.AutoCAD.Colors.Color clr = dlg.Color;
            if (!clr.IsByAci)
            {
                if (clr.IsByLayer)
                {
                    return "ByLayer";
                }

                if (clr.IsByBlock)
                {
                    return "ByBlock";
                }

                string colorName = $"{clr.Red},{clr.Green},{clr.Blue}";

                //if (ClosureTypes.Contains(colorName) == false)
                //    ClosureTypes.Insert(ClosureTypes.Count - 1, new AcadClosure(eClosureType.NewClosure));

                return colorName;
            }
            else
            {
                short colIndex = clr.ColorIndex;

                Byte byt = Convert.ToByte(colIndex);
                int rgb = Autodesk.AutoCAD.Colors.EntityColor.LookUpRgb(byt);
                long b = (rgb & 0xffL);
                long g = (rgb & 0xff00L) >> 8;
                long r = rgb >> 16;

                string colorName = $"Color {colIndex}";

                //if (ClosureTypes.Contains(colorName) == false)
                //    ClosureTypes.Insert(ClosureTypes.Count - 1, new AcadClosure());

                return colorName;
            }
        }

        public void Reload()
        {
            ClosureTypes = new AcadClosures();
            ClosureType = ClosureTypes.FirstOrDefault();
        }

        public void NewClosure(eClosureType closureType)
        {
        }
        private void OnCurrentElementChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("IsAddTitleEnabled");
        }
        #endregion
    }
}