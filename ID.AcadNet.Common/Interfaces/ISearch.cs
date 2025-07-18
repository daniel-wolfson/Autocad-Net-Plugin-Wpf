namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface ISearch
    {
        string SearchKeyword { get; }
        bool IsSearchEnabled { get; }
        string MessageInfo { get; }

        void Search(string keyword);
        void Cancel();
    }
}
