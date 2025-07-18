using ID.Infrastructure.Enums;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.Data.Models.Entities;
using System.Collections.ObjectModel;
using System.Linq;

namespace Intellidesk.AcadNet.ViewModels
{
    public class ClosureConnectPanelElementContext : PaletteElementContext
    {
        ObservableCollection<PaletteElement> MarkerTypes;

        public LayerViewModel MarkerLayerDataContext { get; set; }

        public ClosureConnectPanelElementContext(eOpenCloseType type)
        {
            CurrentElement = ElementItems.FirstOrDefault(x => x.TypeCode == (int?)type);
        }

        #region <override methods>
        public void Load()
        {
            //base.Load();
            ElementItems = new ObservableRangeCollection<PaletteElement>
            {
                new PaletteElement(eBodyType.Rectangle),
                new PaletteElement(eBodyType.Donut)

            };

            MarkerTypes = new ObservableRangeCollection<PaletteElement>
            {
                new AcadClosureConnect(eOpenCloseType.Open),
                new AcadClosureConnect(eOpenCloseType.Close)
            };

            CurrentElement = ElementItems.FirstOrDefault(); //x => x.TypeCode == elementType

            CurrentElement = new PaletteElement(typeof(eBodyType), (int?)eBodyType.Rectangle)
            {
                OwnerFullType = typeof(AcadClosureConnect).FullName
            };

        }
        #endregion

        public void NewClosure(eOpenCloseType closureType)
        {
        }
    }
}