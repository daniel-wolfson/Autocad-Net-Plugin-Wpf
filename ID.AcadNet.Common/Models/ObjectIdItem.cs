using Autodesk.AutoCAD.DatabaseServices;

namespace Intellidesk.AcadNet.Common.Models
{
    public class ObjectIdItem
    {
        public ObjectIdItem()
        {
            ObjectId = ObjectId.Null;
            DisplayName = "none";
        }

        public ObjectIdItem(ObjectId objectId, string displayName)
        {
            ObjectId = objectId;
            DisplayName = displayName;
        }

        public ObjectId ObjectId { get; set; }
        public string DisplayName { get; set; }
    }
}
