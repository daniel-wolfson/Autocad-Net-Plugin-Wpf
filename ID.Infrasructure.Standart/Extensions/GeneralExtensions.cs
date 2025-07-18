using System.Collections.Generic;

namespace ID.Infrastructure.Extensions
{
    public static class GeneralExtensions
    {
        public static bool TryAdd(this IDictionary<object, object> dic, object key, object value)
        {
            try
            {
                if (!dic.ContainsKey(key))
                {
                    dic.Add(key, value);
                    return true;
                }
            }
            catch { }

            return false;
        }
    }
}
