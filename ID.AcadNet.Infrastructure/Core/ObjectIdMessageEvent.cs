using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Prism.Events;


namespace ID.AcadNet.Infrastructure.Core
{
    public class ObjectIdMessageEvent : PubSubEvent<List<ObjectId>> { }
}
