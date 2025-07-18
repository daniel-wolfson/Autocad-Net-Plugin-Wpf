using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.Data.Models.Cad;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Rule = Intellidesk.Data.Models.Cad.Rule;
using UserSetting = Intellidesk.Data.Models.Cad.UserSetting;

namespace Intellidesk.AcadNet.Common.Core
{
    public interface IProjectExplorerPanelContext : IBasePanelContext
    {
        IFileExplorerViewModel FileExplorerViewModel { get; }
        void RibbonRefresh();
        RibbonButton CurrentRibbonLayout { get; set; }
        string[] LayoutsContentsItems { get; set; }

        ObservableCollection<Config> ConfigItems { get; }
        ObservableCollection<ILayout> LayoutItems { get; }
        ObservableCollection<Rule> RuleItems { get; }
        ObservableCollection<Filter> LayoutFilterItems { get; }
        ObservableCollection<UserSetting> UserSettingItems { get; }
        ObservableCollection<RibbonButton> LayoutItemButtons { get; }

        ILayout CurrentLayout { get; set; }
        bool IsTabButtonActive { get; set; }
        bool IsLoaded { get; set; }
        ICommand CloseCommand { get; }
        ICommand RefreshCommand { get; }
        void OnDocumentActivated(object sender, DocumentCollectionEventArgs e);
        ISearchViewModel SearchViewModel { get; }
        ICommand AddToLayotsCommand { get; }
        ICommand GoToFolderCommand { get; }
        ICommand RemoveLayoutCommand { get; }
        ICommand SetFolderBaseCommand { get; }
        Config CurrentConfig { get; }
    }
}