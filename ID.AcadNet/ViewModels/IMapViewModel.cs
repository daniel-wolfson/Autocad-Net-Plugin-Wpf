using System.Collections.ObjectModel;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.AcadNet.ViewModels
{
    public interface IMapViewModel
    {
        ILayout CurrentLayout { get; set; }
        RibbonButton CurrentRibbonTab { get; set; }
        User CurrentUser { get; set; }
        MapViewModel DataContext { get; set; }
        string Error { get; }
        bool IsLayoutReadOnly { get; set; }
        bool IsTabClosed { get; set; }
        ObservableCollection<ILayout> LayoutItems { get; set; }
        short ToggleLayoutDataTemplateSelector { get; set; }

        ICommand CreateCommand { get; }
        ICommand CloseCommand { get; }
        ICommand OpenCommand { get; }
        ICommand RefreshCommand { get; }
        ICommand SaveCommand { get; }

        void OnDocumentActivated(object sender, DocumentCollectionEventArgs e);
    }
}