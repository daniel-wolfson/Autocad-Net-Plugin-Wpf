using ID.Infrastructure.Core;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace ID.Infrastructure.Extensions
{
    public static class CacheExtensions
    {
        public static void CacheReset(this IMemoryCache cacheResults)
        {
            cacheResults = new MemoryCache(new MemoryCacheOptions());
        }

        public static void CacheClear<TResult>(this IMemoryCache cacheResults, string path)
        {
            var cacheKey = path + "_" + typeof(TResult).ToString();
            cacheResults.Remove(cacheKey);
        }

        public static void CacheClearAll<TResult>(this IMemoryCache cacheResults, string path)
        {
            Type modelType = GetModelType<TResult>();
            if (!modelType.IsValueType || GeneralContext.ControllerNames.Any(path.Contains))
            {
                var cacheKeys = GeneralContext.CacheResults.Keys.Where(t => t.Contains(modelType.Name) || GeneralContext.ControllerNames.Any(t.Contains)).ToList();
                foreach (var cacheKey in cacheKeys)
                {
                    cacheResults.Remove(cacheKey);
                }
            }
        }

        public static string CacheCreateKey<TResult>(this IMemoryCache cacheResults, string path)
        {
            Type fullType = typeof(TResult);
            Type modelType = GetModelType<TResult>();
            return string.Join('_', path, fullType.Name, modelType);
        }

        private static Type GetModelType<TResult>()
        {
            Type resultType;
            Type type = typeof(TResult);
            if (type.IsGenericType && type.GenericTypeArguments.Length == 1)
                resultType = type.GenericTypeArguments[0];
            else
                resultType = type;
            return resultType;
        }

        //public static TItem CacheGet<TItem>(this IMemoryCache cache, object key) { }
        //public static TItem CacheSet<TItem>(this IMemoryCache cache, object key, TItem value, MemoryCacheEntryOptions options) { }
        //public static bool CacheTryGetValue<TItem>(this IMemoryCache _cache, object key, out TItem value) { 

        //    object cacheEntry;
        //    // Look for cache key.
        //    if (!_cache.TryGetValue(key, out cacheEntry))
        //    {
        //        // Key not in cache, so get data.
        //        cacheEntry = default(object);
        //        // Set cache options.
        //        var cacheEntryOptions = new MemoryCacheEntryOptions()
        //            // Keep in cache for this time, reset time if accessed.
        //            .SetSlidingExpiration(TimeSpan.FromSeconds(3));
        //        // Save data in cache.
        //        _cache.Set(key, cacheEntry, cacheEntryOptions);
        //    }
        //    return true;
        //}
    }
}
