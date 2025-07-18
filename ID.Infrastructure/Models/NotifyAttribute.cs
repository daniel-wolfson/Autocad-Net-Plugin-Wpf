using System;
using System.Reflection;

namespace ID.Infrastructure.Models
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NotifyAttribute : Attribute
    {
        private readonly string _name;
        public NotifyAttribute(string name)
        {
            _name = name;
        }

        public static string Get(Type tp, string name)
        {
            MemberInfo[] mi = tp.GetMember(name);
            if (mi.Length > 0)
            {
                NotifyAttribute attr = GetCustomAttribute(mi[0],
                    typeof(NotifyAttribute)) as NotifyAttribute;
                if (attr != null) return attr._name;
            }
            return null;
        }
        public static string Get(object enm)
        {
            MemberInfo[] mi = enm?.GetType().GetMember(enm.ToString());
            if (mi?.Length > 0)
            {
                NotifyAttribute attr = GetCustomAttribute(mi[0],
                    typeof(NotifyAttribute)) as NotifyAttribute;
                if (attr != null) return attr._name;
            }
            return null;
        }
    }
}