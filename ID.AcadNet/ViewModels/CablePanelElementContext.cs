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
    public class CablePanelElementContext : BaseViewModel
    {
        #region <props>
        private AcadCables _cableTypes;
        public AcadCables CableTypes
        {
            get { return _cableTypes; }
            set
            {
                _cableTypes = value;
                OnPropertyChanged();
            }
        }
        private AcadCable _cableType;
        public AcadCable CableType
        {
            get { return _cableType; }
            set
            {
                _cableType = value;
                OnPropertyChanged();
            }
        }
        private AcadCable _currentCable;
        public AcadCable CurrentElement
        {
            get { return _currentCable; }
            set
            {
                _currentCable.PropertyChanged -= OnCurrentElementChanged;
                _currentCable = value;
                _currentCable.PropertyChanged += OnCurrentElementChanged;
                OnPropertyChanged();
            }
        }

        public bool IsAddTitleEnabled =>
            !string.IsNullOrEmpty(CurrentElement.Title) &&
            !string.IsNullOrEmpty(CurrentElement.Handle) &&
            CurrentElement.Items.Length > 0;

        #endregion 

        #region <ctor>
        public CablePanelElementContext(eCableType cableType)
        {
            _cableTypes = new AcadCables(); //Enum.GetValues(typeof(eCableType)).Cast<eCableType>().ToList(); 
            _cableType = _cableTypes.FirstOrDefault(x => x.TypeCode == cableType);
            _currentCable = new AcadCable(cableType);
        }
        #endregion

        #region <methods>
        public string ShowCableDlg()
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

                //if (CableTypes.Contains(colorName) == false)
                //    CableTypes.Insert(CableTypes.Count - 1, new AcadCable(eCableType.NewCable));

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

                string colorName = string.Format("Color {0}", colIndex);

                //if (CableTypes.Contains(colorName) == false)
                //    CableTypes.Insert(CableTypes.Count - 1, new AcadCable());

                return colorName;
            }
        }
        public void Reload()
        {
            CableTypes = new AcadCables();
            CableType = CableTypes.FirstOrDefault();
        }
        public void NewCable(eCableType cableType)
        {
        }

        private void OnCurrentElementChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("IsAddTitleEnabled");
        }
        #endregion 
    }
}