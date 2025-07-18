using Autodesk.AutoCAD.ApplicationServices;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.Data.Models.Cad;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface IPanelContext : IBasePanelContext, IEventable
    {
        ObservableRangeCollection<ObjectIdItem> ElementItems { get; set; }
        List<string> Layers { get; set; }

        string Header { get; set; }
        string RunButtonText { get; set; }
        //string SelectedText { get; set; }
        int CurrentZoomDisplayFactor { get; set; }
        List<string> ScaleFactors { get; set; }
        int DetailsCount { get; set; }
        ObjectIdItem SelectedKey { get; set; }
        ObjectIdItem SelectedItem { get; set; }

        double ProgressBarMinimum { get; }
        double ProgressBarMaximum { get; set; }
        double ProgressBarValue { get; set; }
        bool CanPopulated { get; set; }
        UserSetting CurrentUserSetting { get; set; }

        bool IsCanceled { get; set; }
        bool IsLoaded { get; set; }

        ICommand RunCommand { get; }
        ICommand EditCommand { get; }
        ICommand SelectSetCommand { get; }
        ICommand AddCommand { get; }
        ICommand AddTitleCommand { get; }
        ICommand UndoCommand { get; }
        ICommand RefreshCommand { get; }
        ICommand CloseCommand { get; }
        ICommand GetLocationCommand { get; }

        //void ExecuteSelectSetCommand(object commandParameter);
        //void ExecuteRunCommand(object commandParameter);
        //void ExecuteResetCommand(object commandParameter);

        /// <summary> returnig is dwg conpatible base on lsds rules </summary>
        bool IsDwgCompatible(Type[] typeFilterOn = null, string[] attributePatternOn = null);

        bool IsDwgOpen(Type[] typeFilterOn = null, string[] attributePatternOn = null);

        bool SwitchSizeMode { get; set; }

        bool OnFindAction(ITaskArgs args);
        void OnDocumentActivated(object sender, DocumentCollectionEventArgs e);
        void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e);
        void OnAcadEntityErased(object sender, EventArgs e);
        void OnAcadEntityModified(object sender, EventArgs e);

        //void LoadEvents();
        //void RemoveEvents();
    }
}