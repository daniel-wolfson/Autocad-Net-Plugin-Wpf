using Intellidesk.Data.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using AcadLayer = Intellidesk.AcadNet.Common.Models.AcadLayer;

namespace Intellidesk.AcadNet.ViewModels
{
    /// <summary>
    /// Class used to bind the combobox selections to. Must implement 
    /// INotifyPropertyChanged in order to get the data binding to 
    /// work correctly.
    /// </summary>
    public class LayerViewModel : BaseEntity
    {
        private AcadLayer _currentLayer;

        public List<AcadLayer> Layers { get; private set; }

        public LayerViewModel(string elementType, string layerName)
        {
            Load(elementType);
            _currentLayer = Layers.FirstOrDefault(x => x.LayerName == layerName);
            IsReadOnly = true;
        }

        /// <summary> String property used in binding examples. </summary>
        public AcadLayer CurrentLayer
        {
            get { return _currentLayer; }
            set
            {
                if (value != null && !Equals(_currentLayer, value))
                {
                    _currentLayer = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Load(string layerType)
        {
            Layers = AcadNetManager.Layers.Where(x => x.LayerType == layerType).ToList();
        }
    }
}
