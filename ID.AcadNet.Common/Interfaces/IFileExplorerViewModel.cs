using System.Collections.Generic;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IFileExplorerViewModel
    {
        IFolder BaseFolder { get; set; }
        IList<string> CheckedPaths { get; }
        IFolder Computer { get; set; }
        IFile CurrentFile { get; }
        IFolder CurrentFolder { get; set; }
        bool IsExplorerInitialized { get; }
        bool IsLoading { get; set; }
        System.Collections.ObjectModel.ObservableCollection<IFolder> Items { get; set; }
        ICommand OpenFileCommand { get; }
        ICommand RefreshFolderDetailsCommand { get; }
        ISearchViewModel SearchViewModel { get; set; }
        IFolder SelectedFolder { get; set; }
        ICommand SelectedFolderCommand { get; }
        ICommand SelectionFileCommand { get; }
        bool SelectSaveEnable { get; set; }
        bool ShowAll { get; set; }
        ICommand SortFilesCommand { get; }

        void ExecuteSelectedFolder(IFolder folder);
        IEnumerable<string> GetCheckedPaths();
        System.Threading.Tasks.Task<IFolder> LoadFolderChildren(IFolder folder);
        IFolder LoadLocalExplorer();
        void SetCheckedPaths(IList<string> pathList, bool isChecked = true);
        void SetCurrentFolder(IFolder folder);
        void SetPathChecked(IEnumerable<string> list, bool isChecked = true);
    }
}