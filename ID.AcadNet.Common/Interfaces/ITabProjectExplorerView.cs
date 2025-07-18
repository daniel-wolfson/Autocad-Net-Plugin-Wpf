namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface ITabProjectExplorerView : IPanelTabView
    {
        //MapViewModel DataContext { set; get; }
        //void SetSelectedFolder(IFolder folder);
        void ExpandFolder(IFolder folder, bool loadLocalExpolorer = false);
    }
}