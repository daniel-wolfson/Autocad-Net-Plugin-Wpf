using ID.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ID.Infrastructure.Interfaces
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string cacheKey) where T : ApiCacheItem;
        Dictionary<string, TCacheItem> GetCacheItems<TCacheItem>() where TCacheItem : class;
        Task SetAsync<T>(string cacheKey, T cacheItem) where T : ApiCacheItem;
        Task SetAsync(string key, string query, object data, bool scoped = true, bool preLoad = true);

        bool IsCancellationRequested { get; }
        Task RefreshCacheAsync(string cacheKey = null);
        void RemoveKey(string cacheKey);
        void RemoveKeyType(string typeName);
        void Remove<TCacheItem>() where TCacheItem: class;
        void RemoveAll();

        ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default);
        ValueTask WriteAsync(ApiCacheItem item, CancellationToken cancellationToken = default);
        ValueTask<ApiCacheItem> ReadAsync(CancellationToken cancellationToken = default);
        Task<bool> PreloadAsync(IServiceProvider serviceProvider);
        Task<bool> ReloadAsync(IServiceProvider serviceProvider);

        bool CacheLoading { get; set; }
        bool CacheLoaded { get; set; }

        bool ContainsKey(string cacheKey);
        bool ContainsKeyType(string cacheKey);
        bool ContainsPath(string path);

        string CreateCacheKey(string path, Type entityType);
        string CreateCacheKey(string path, string entityTypeName);
        string CreateCacheKey(MethodInfo methodName, Type methodResultType);

        Type GetModelType<TResult>();
        Type GetModelType(Type type);
    }


}

