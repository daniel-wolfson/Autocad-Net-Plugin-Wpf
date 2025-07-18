using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using System.Collections.Generic;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface ILayerQueriesPanelContext : IBasePanelContext
    {
        string CurrentSourceLayer { get; set; }
        string CurrentMachineLayer { get; set; }
        string CurrentMachineName { get; set; }
        string CurrentProcessName { get; set; }
        string CurrentMachineScope { get; set; }
        int CurrentMachineRampFrom { get; set; }
        int CurrentMachineRampTo { get; set; }
        ObjectIdItem SelectedItem { get; set; }
        List<string> SourceLayers { get; }
        List<string> MachineLayers { get; }
        List<string> MachineNames { get; }
        List<string> ProcessNames { get; }
        List<string> MachineScopes { get; }
        Dictionary<string, int> MachineRamps { get; }
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