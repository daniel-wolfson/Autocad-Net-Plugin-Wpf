using Intellidesk.AcadNet.Core;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface ITabViewLayout : Common.Interfaces.IPanelTabView
    {
        event UIControlClickEventHandler OpenClickEvent;
        event UIControlClickEventHandler SaveClickEvent;
        event UIControlClickEventHandler DeleteClickEvent;
        event UIControlClickEventHandler CopyClickEvent;
        event UIControlClickEventHandler ParseClickEvent;
        event UIControlClickEventHandler PurgeClickEvent;
        event UIControlClickEventHandler SaveChangedEvent;
    }
}