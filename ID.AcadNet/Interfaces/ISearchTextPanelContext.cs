using Autodesk.AutoCAD.ApplicationServices;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface ISearchTextPanelContext : IBasePanelContext, IBaseViewModel
    {
        string SelectedText { get; set; }
        int CurrentZoomDisplayFactor { get; set; }
        List<string> ScaleFactors { get; set; }
        string CurrentLayer { get; set; }
        List<string> Layers { get; set; }
        ObjectIdItem SelectedKey { get; set; }
        ObjectIdItem SelectedItem { get; set; }
        double ProgressBarMinimum { get; }
        double ProgressBarMaximum { get; set; }
        double ProgressBarValue { get; set; }
        bool CanPopulated { get; set; }
        bool IsCanceled { get; set; }
        ObservableRangeCollection<ObjectIdItem> ExistListItems { get; set; }

        ICommand RunCommand { get; }
        ICommand SelectSetCommand { get; }
        ICommand ExportCommand { get; }
        ICommand UndoCommand { get; }
        ICommand CloseCommand { get; }
        ICommand ClearCommand { get; }
        ICommand RefreshCommand { get; }

        void ExecuteSelectSetCommand(object commandParameter);
        void ExecuteRunCommand(string commandParameter);
        void ExecuteProgressResetCommand(object commandParameter);

        /// <summary> returnig is dwg conpatible base on lsds rules </summary>
        bool IsDwgCompatible(Type[] typeFilterOn = null, string[] attributePatternOn = null);
        bool IsDwgOpen(Type[] typeFilterOn = null, string[] attributePatternOn = null);

        bool OnFindAction(ITaskArgs args);
        void OnDocumentActivated(object sender, DocumentCollectionEventArgs e);
        void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e);
    }
}