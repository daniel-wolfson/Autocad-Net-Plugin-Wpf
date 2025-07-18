using System.ComponentModel;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public delegate void SortOrderCallback(bool isRefresh);

    public interface ISortOrder
    {
        void SetSortOrderCallback(SortOrderCallback callback);
        void SetSortOrder(string propName, ListSortDirection sortDirection);
    }
}
