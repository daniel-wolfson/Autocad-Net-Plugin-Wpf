using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.Data.Models.Entities;
using System;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.ViewModels
{
    public class PaletteElementContext : BaseViewModel
    {
        #region <field>
        private PaletteElement _currentElement;
        private ColorViewModel _bodyColorDataContext;
        private Action<PaletteElementContext> _loadAction;
        #endregion <field>

        public virtual void Load(Action<PaletteElementContext> action)
        {
            _loadAction = action;
            _loadAction(this);
        }
        public virtual void Reload()
        {
            _loadAction(this);
        }

        #region <props>

        public LayerViewModel LayerDataContext { get; set; }
        public ColorViewModel ColorDataContext
        {
            get => _bodyColorDataContext;
            set
            {
                _bodyColorDataContext = value;
                OnPropertyChanged();
            }
        }

        private ObservableRangeCollection<PaletteElement> _elementItems = new ObservableRangeCollection<PaletteElement>();
        public new ObservableRangeCollection<PaletteElement> ElementItems
        {
            get => _elementItems;
            set
            {
                _elementItems = value;
                OnPropertyChanged();
                DetailsCount = _elementItems.Count;
            }
        }

        public PaletteElement CurrentElement
        {
            get { return _currentElement; }
            set
            {
                _currentElement = value;
                OnPropertyChanged();
            }
        }

        public bool IsAddTitleEnabled =>
            !string.IsNullOrEmpty(CurrentElement.Title) &&
            !string.IsNullOrEmpty(CurrentElement.Handle) &&
            CurrentElement.Items.Length > 0;

        #endregion <props>

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
                //    ClosureTypes.Insert(ClosureTypes.Count - 1, new AcadClosure(eConnectorType.NewClosure));

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
        #endregion <methods>
    }
}