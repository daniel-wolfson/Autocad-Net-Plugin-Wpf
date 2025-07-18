using Intellidesk.AcadNet.Common.Models;
using System.Collections;
using System.Collections.ObjectModel;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface ILayerService : ICollection
    {
        void Create(ObservableCollection<AcadLayer> listItems = null);

        AcadLayer Current();
        ObservableCollection<AcadLayer> GetAll();

        void Freeze(string value, bool tFreeze);
        bool IsOnOff(string value);
        void OnOff(string value, bool tOnOff);
        void ListLayerFilters();
        void CreateLayerFilters();
        void DeleteLayerFilter();
        void ChangeLayerNamesDynamically();
        string GetWorkLayerName(string layerName);
        bool Contains(string layerName);
        AcadLayer Get(string layerName);
        void Add(string layerName);
        void Load();
    }
}