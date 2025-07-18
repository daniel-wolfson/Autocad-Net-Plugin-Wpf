using Prism.Events;

namespace ID.Infrastructure.Models
{
    public class NotifyMessageStringEvent : PubSubEvent<string> { }

    public class NotifyMessageEvent : PubSubEvent<NotifyArgs> { }

    public class NotifyMessageHandleEvent : PubSubEvent<long> { }
}