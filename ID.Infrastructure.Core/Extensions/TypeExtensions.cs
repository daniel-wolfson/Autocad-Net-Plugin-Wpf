using System;
using System.Reflection;

namespace ID.Infrastructure.Extensions
{
    public static class TypeExtensions
    {
        // var defaultValue = targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        public static object GetDefault(this Type t)
        {
            var defaultValue = typeof(TypeExtensions)
                .GetRuntimeMethod(nameof(GetDefaultGeneric), new Type[] { })
                .MakeGenericMethod(t).Invoke(null, null);
            return defaultValue;
        }

        public static T GetDefaultGeneric<T>()
        {
            return default(T);
        }
    }
}
