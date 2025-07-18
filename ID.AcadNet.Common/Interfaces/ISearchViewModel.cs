using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface ISearchViewModel
    {
        StringBuilder ActionMessage { get; }
        int AllCount { get; }
        int DocumentCount { get; set; }
        int ImageCount { get; set; }
        bool? IsChecked { get; set; }
        bool IsMessageInfoVisible { get; }
        bool IsSearchCompleted { get; }
        bool IsSearchIncludedSubDir { get; }
        bool IsSearchEnabled { get; set; }
        bool IsSearching { get; set; }
        string MessageInfo { get; set; }
        int MusicCount { get; set; }
        string SearchKeyword { get; set; }
        int VideoCount { get; set; }
        void Cancel();
        IEnumerable<IFile> GetCheckedItems();
        void InitialSearch(IFolder rootItem);
        void Search(string newKeyword);
        void UninitialSearch();

        IBasePanelContext Parent { get; set; }
        ICommand SearchClearCommand { get; }
        ICommand SearchCommand { get; }
    }
}