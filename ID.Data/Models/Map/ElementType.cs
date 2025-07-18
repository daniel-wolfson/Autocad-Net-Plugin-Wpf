namespace Intellidesk.Data.Models.Map
{
    public class ElementType
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }

        public ElementType(int typeId, string typeName)
        {
            TypeId = typeId;
            TypeName = typeName;
        }
    }
}