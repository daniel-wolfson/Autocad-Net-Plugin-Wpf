using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.ViewModels
{
    public class CabinetPanelElementContext : BaseViewModel, ICabinetPanelContext
    {
        #region <props>
        private AcadCabinets _cabinetTypes;
        public AcadCabinets CabinetTypes
        {
            get { return _cabinetTypes; }
            set
            {
                _cabinetTypes = value;
                OnPropertyChanged();
            }
        }
        private AcadCabinet _cabinetType;
        public AcadCabinet CabinetType
        {
            get { return _cabinetType; }
            set
            {
                _cabinetType = value;
                OnPropertyChanged();
            }
        }
        private AcadCabinet _currentCabinet;
        public AcadCabinet CurrentElement
        {
            get { return _currentCabinet; }
            set
            {
                _currentCabinet.PropertyChanged -= OnCurrentElementChanged;
                _currentCabinet = value;
                _currentCabinet.PropertyChanged += OnCurrentElementChanged;
                OnPropertyChanged();
            }
        }

        public bool IsAddTitleEnabled =>
            !string.IsNullOrEmpty(CurrentElement.Title) &&
            !string.IsNullOrEmpty(CurrentElement.Handle) &&
            CurrentElement.Items.Length > 0;

        public CabinetPanelElementContext BodyElementDataContext => throw new NotImplementedException();

        public ISearchViewModel SearchViewModel => throw new NotImplementedException();

        public List<string> Layers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action FinishInteraction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Header { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RunButtonText { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int CurrentZoomDisplayFactor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ObjectIdItem SelectedKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ObjectIdItem SelectedItem { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool CanPopulated { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICommand RunCommand => throw new NotImplementedException();

        public ICommand EditCommand => throw new NotImplementedException();

        public ICommand SelectSetCommand => throw new NotImplementedException();

        public ICommand AddCommand => throw new NotImplementedException();

        public ICommand AddTitleCommand => throw new NotImplementedException();

        public ICommand UndoCommand => throw new NotImplementedException();

        public ICommand RefreshCommand => throw new NotImplementedException();

        public ICommand CloseCommand => throw new NotImplementedException();

        public ICommand GetLocationCommand => throw new NotImplementedException();

        public Action<object> Load { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        #region <ctor>
        public CabinetPanelElementContext(eCabinetType cabinetType)
        {
            _cabinetTypes = new AcadCabinets(); //Enum.GetValues(typeof(eCabinetType)).Cast<eCabinetType>().ToList();
            _cabinetType = _cabinetTypes.FirstOrDefault(x => x.TypeCode == cabinetType);
            _currentCabinet = new AcadCabinet(cabinetType);
        }
        #endregion

        #region <methods>
        public string ShowCabinetDlg()
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

                //if (CabinetTypes.Contains(colorName) == false)
                //    CabinetTypes.Insert(CabinetTypes.Count - 1, new AcadCabinet());

                return colorName;
            }
        }
        public void Reload()
        {
            //CabinetTypes = Enum.GetValues(typeof(eCabinetType)).Cast<eCabinetType>().ToList();
            //CabinetType = CabinetTypes.FirstOrDefault();
        }
        public void NewCabinet(eCabinetType cabinetType)
        {

        }

        private void OnCurrentElementChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("IsAddTitleEnabled");
        }

        public bool OnFindAction(ITaskArgs args)
        {
            throw new NotImplementedException();
        }

        public void ExecuteRefreshCommand(object value)
        {
            throw new NotImplementedException();
        }

        public Task<ICommandArgs> ExecuteCommand(ICommandArgs command)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}