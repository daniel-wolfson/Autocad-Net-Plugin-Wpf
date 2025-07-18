using System;

namespace Intellidesk.Data.Models
{
    public class EntityChangedArgs : EventArgs
    {
        public EntityChangedArgs(object value, string propName, bool isValid)
        {
            IsValid = isValid;
            PropName = propName;
            Current = value;
        }

        protected bool IsValid { get; set; }
        public string PropName { get; set; }
        //public static List<string> ChangedProperties { get; set; }
        public object Current { get; set; }
    }
}