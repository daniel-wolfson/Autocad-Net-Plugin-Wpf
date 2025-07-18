namespace Intellidesk.Data.Models.Dto
{
    public class AcadSysVar
    {
        public string Name { set; get; }
        public object Value { set; get; }

        public AcadSysVar(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
