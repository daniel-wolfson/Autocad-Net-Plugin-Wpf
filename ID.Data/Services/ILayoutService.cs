using System.Collections.Generic;
using System.Collections.ObjectModel;
using ILayout = Intellidesk.Data.Models.Cad.ILayout;

namespace Intellidesk.Data.Services
{
    /// <summary> Add any custom business logic (methods) here </summary>
    public interface ILayoutService : IService<ILayout>
    {
        decimal CustomerOrderTotalByYear(string customerId, int year);
        IEnumerable<ILayout> LoadLayoutByFileName(string companyName);
        IEnumerable<ILayout> GetLayouts(bool fromCache = false);
        void AddAndSave(string fullPath, string work, string commandType);
    }
}