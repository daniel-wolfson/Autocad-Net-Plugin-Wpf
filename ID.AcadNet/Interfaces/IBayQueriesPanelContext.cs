using Autodesk.AutoCAD.DatabaseServices;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.ViewModels;
using System.Collections.Generic;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface IBayQueriesPanelContext : IBasePanelContext
    {
        ObjectId CurrentBay { get; set; }
        ObjectIdItem SelectedItem { get; set; }
        List<Bay> Bays { get; }

        double ProgressBarMaximum { get; set; }
        double ProgressBarMinimum { get; set; }
        double ProgressBarValue { get; set; }
        double ProgressStateValue { get; set; }

        ObservableRangeCollection<ObjectIdItemArgs> ExistListItems { get; set; }

        ICommand RefreshCommand { get; set; }
        ICommand RunCommand { get; set; }
        ICommand StopCommand { get; set; }
        ICommand AddCommand { get; set; }
        ICommand ResetCommand { get; set; }
        ICommand CloseCommand { get; set; }
        ICommand SelectSetCommand { get; set; }
        ICommand ExportCommand { get; set; }
        ICommand ClearCommand { get; set; }
    }
}