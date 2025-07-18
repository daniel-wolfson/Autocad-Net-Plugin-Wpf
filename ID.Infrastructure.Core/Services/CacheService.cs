using ID.Infrastructure.Controllers;
using ID.Infrastructure.Core;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Filters;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ID.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        private readonly ChannelWriter<ApiCacheItem> _channelWriter;
        private readonly ChannelReader<ApiCacheItem> _channelReader;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;
        private CancellationToken _stoppingToken;

        public bool CacheLoaded { get; set; } = false;
        public bool CacheLoading { get; set; } = false;
        public bool IsCancellationRequested => _cancellationToken.IsCancellationRequested || _stoppingToken.IsCancellationRequested;

        public CacheService(IDistributedCache cache, Channel<ApiCacheItem> channel, ILogger<CacheService> logger)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _channelWriter = channel.Writer;
            _channelReader = channel.Reader;
            _cache = cache;
            _logger = logger;
        }

        #region public methods

        /// <summary> Get cache by key</summary>
        public async Task<T> GetAsync<T>(string cacheKey) where T : ApiCacheItem
        {
            var cacheItem = await _cache.GetAsync(cacheKey.ToLower());
            T cacheItemObject = Util.ConvertByteArrayToObject(cacheItem) as T;
            return cacheItemObject;
        }

        /// <summary> Set cache</summary>
        public Task SetAsync<T>(string cacheKey, T item) where T : ApiCacheItem
        {
            return _cache.SetAsync(cacheKey.ToLower(), Util.ConvertObjectToByteArray(item));
        }

        /// <summary> Set cache</summary>
        public Task SetAsync(string key, string query, object data, bool scoped = true, bool preLoad = true)
        {
            var item = new ApiCacheItem(data, preLoad, scoped, query);
            return _cache.SetAsync(key.ToLower(), Util.ConvertObjectToByteArray(item));
        }

        /// <summary> Remove cache item by key</summary>
        public void RemoveKey(string cacheKey)
        {
            _cache.Remove(cacheKey.ToLower());
        }

        public void RemoveKeyPath(string path)
        {
            var cacheKeys = this.GetCacheItems<object>().Keys;
            foreach (var cacheKey in cacheKeys.Where(x => x.Contains(path.ToLower())))
            {
                _cache.Remove(cacheKey);
            }
        }

        public void RemoveKeyType(string typeName)
        {
            var cacheKeys = this.GetCacheItems<object>().Keys;
            foreach (var cacheKey in cacheKeys.Where(x => x.Contains("$" + typeName.ToLower())))
            {
                _cache.Remove(cacheKey);
            }
        }

        /// <summary> Remove cache item by type</summary>
        public void Remove<TCacheItem>() where TCacheItem : class
        {
            var cacheKeys = this.GetCacheItems<TCacheItem>().Keys.ToList();
            foreach (var cacheKey in cacheKeys)
            {
                _cache.Remove(cacheKey);
            }
        }

        /// <summary> Remove all cache</summary>
        public void RemoveAll()
        {
            var cacheKeys = this.GetCacheItems<object>().Keys;
            foreach (var cacheKey in cacheKeys)
            {
                _cache.Remove(cacheKey);
            }
        }

        public bool ContainsKey(string cacheKey)
        {
            var cacheItems = this.GetCacheItems<ApiCacheItem>();
            return cacheItems.Keys.Any(x => x == cacheKey.ToLower());
        }

        /// <summary> Define that cache contains key</summary>
        public bool ContainsKeyType(string cacheKey)
        {
            var cacheItems = this.GetCacheItems<ApiCacheItem>();
            return cacheItems.Keys.Any(k => k.Contains("$" + cacheKey.ToLower()));
        }

        public bool ContainsPath(string path)
        {
            var cacheItems = this.GetCacheItems<ApiCacheItem>();
            return cacheItems.Keys.Any(key => key.ToLower().Contains(path.ToLower()));
        }

        /// <summary> Create cache key by full path to method and type of result</summary>
        public string CreateCacheKey(string path, Type entityType)
        {
            Type modelType = GetModelType(entityType);
            var result = CreateCacheKey(path, modelType.Name);
            return result;
        }

        public string CreateCacheKey(string path, string entityTypeName)
        {
            return $"{path}${entityTypeName}".ToLower();
        }

        public string CreateCacheKey(string serviceName, string controllerName, string methodName, string methodResultTypeName)
        {
            var apiServiceName = $"{typeof(ICacheService).Assembly.GetName().Name.ToLower().Replace("crpm.", "")}Api".ToLower();
            return methodResultTypeName.ToLower();

        }

        /// <summary> Create cache key by method's metadata </summary>
        public string CreateCacheKey(MethodInfo method, Type methodResultType)
        {
            //var apiServiceName = typeof(ICacheService).Assembly.GetName().Name.ToLower().Replace("crpm.", "") + "Api".ToLower();
            Type modelType = GetModelType(methodResultType);
            return modelType.Name.ToLower();
        }

        /// <summary> Get all cache items </summary>
        public Dictionary<string, TCacheItem> GetCacheItems<TCacheItem>() where TCacheItem : class
        {
            //TODO: temporary
            //var obj = new DistributedCacheItem(@"SELECT * FROM ""User""", new object() { }); //UserName = "TestUser", UserGuid = Guid.NewGuid().ToString()
            //cache.Set("GetUsers", Util.ConvertObjectToByteArray(obj));

            var items = new Dictionary<string, TCacheItem>();

            FieldInfo _memCachefield = _cache.GetType().GetField("_memCache", BindingFlags.NonPublic | BindingFlags.Instance);
            if (_memCachefield != null)
            {
                IMemoryCache memoryCache = _memCachefield.GetValue(_cache) as IMemoryCache;
                items = GetCacheItems<TCacheItem>(memoryCache);
            }

            return items;
        }

        private Dictionary<string, TCacheItem> GetCacheItems<TCacheItem>(IMemoryCache memoryCache) where TCacheItem : class
        {
            ICollection cacheEntries;
            var items = new Dictionary<string, TCacheItem>();

            try
            {
                var collectionProperty = memoryCache.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                cacheEntries = collectionProperty.GetValue(memoryCache) as ICollection;

                foreach (var cacheEntry in cacheEntries)
                {
                    var methodInfo = cacheEntry.GetType().GetProperty("Key");
                    var value = cacheEntry.GetType().GetProperty("Value").GetValue(cacheEntry, null);

                    value = value.GetType().GetProperty("Value").GetValue(value, null);
                    var k = methodInfo.GetValue(cacheEntry).ToString();

                    if (typeof(TCacheItem) == typeof(ApiCacheItem))
                        value = Util.ConvertByteArrayToObject(value as byte[]);

                    items.Add(k, value as TCacheItem);
                }
            }
            catch (Exception ex)
            {
                GeneralContext.Logger.Error(ex.GetApiMessageInfo());
            }

            return items;
        }

        /// <summary> Reload data into cache </summary>
        public async Task<bool> ReloadAsync(IServiceProvider serviceProvider)
        {
            var items = this.GetCacheItems<ApiCacheItem>();
            var httpContext = serviceProvider.GetService<IHttpContextAccessor>().HttpContext;
            foreach (var item in items)
            {
                httpContext.Items.Add(item.Key, item.Value);
            }
            return await Task.FromResult<bool>(true);
        }

        /// <summary> Preload data and set cache, TODO: not complete </summary>
        public async Task<bool> PreloadAsync(IServiceProvider serviceProvider)
        {
            this.CacheLoaded = true;

            try
            {
                // get controllers typeOf(ControllerBaseAction), that its have metods with attributes typeOf(ApiCacheAttribute)
                var controllerTypes = Assembly.GetEntryAssembly().ExportedTypes
                        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(GeneralControllerBase)))
                        .Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                        .Any(method => method.CustomAttributes
                                            .Any(attr => attr.AttributeType == typeof(ApiCacheAttribute)
                                                && attr.ConstructorArguments.Count == 2
                                                && attr.ConstructorArguments.LastOrDefault().ArgumentType == typeof(Boolean)
                                                && Convert.ToBoolean(attr.ConstructorArguments.LastOrDefault().Value))
                                            ));

                foreach (var controllerType in controllerTypes)
                {
                    ConstructorInfo firstConstrutor = controllerType
                        .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault();

                    // get parameters of constructor for DI services
                    var parameters = new List<object>();
                    foreach (var param in firstConstrutor.GetParameters())
                    {
                        var service = serviceProvider.GetService(param.ParameterType);
                        parameters.Add(service);
                    }

                    // get methods typeOf(ApiCacheAttribute) x.GetParameters().Length == 0 
                    var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .Where(x => x.CustomAttributes.Any(attr => attr.AttributeType == typeof(ApiCacheAttribute)
                                    && attr.ConstructorArguments.Count == 2
                                    && attr.ConstructorArguments.LastOrDefault().ArgumentType == typeof(Boolean)
                                    && Convert.ToBoolean(attr.ConstructorArguments.LastOrDefault().Value)))
                                .ToList();

                    if (methods.Any())
                    {
                        // create controller object
                        object controllerObject = firstConstrutor.Invoke(parameters.ToArray());

                        foreach (var method in methods)
                        {
                            // method invoke
                            Task<IActionResult> task = method.Invoke(controllerObject, null) as Task<IActionResult>;
                            await task.ConfigureAwait(false);

                            // method result
                            var resultProperty = task.GetType().GetProperty("Result");
                            var resultValue = resultProperty.GetValue(task) as ObjectResult;

                            if (resultValue != null)
                            {
                                ICacheService cacheService = serviceProvider.GetService<ICacheService>();

                                var attr = method.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(ApiCacheAttribute));
                                if (attr != null && attr.ConstructorArguments.Count == 2)
                                {
                                    //.Where(x => Convert.ToBoolean(x.ConstructorArguments.FirstOrDefault().Value))
                                    var isPreload = Convert.ToBoolean(attr.ConstructorArguments[1].Value);
                                    //var isScoped = Convert.ToBoolean(attr.ConstructorArguments[1].Value);

                                    // create cacheItem from result
                                    var apiServiceName = typeof(ICacheService).Assembly.GetName().Name + "Api";
                                    var cacheValue = new ApiCacheItem(resultValue.Value, isPreload);
                                    var cacheKey = cacheService.CreateCacheKey(method, resultValue.Value.GetType());

                                    // set http items for quick access from http context
                                    var httpContext = serviceProvider.GetService<IHttpContextAccessor>().HttpContext;
                                    httpContext.Items[cacheKey.ToLower()] = resultValue.Value;

                                    // set cache
                                    await cacheService.SetAsync(cacheKey, cacheValue);
                                }
                            }
                        }
                    }
                }

                this.CacheLoading = false;
                this.CacheLoaded = true;

                return await Task.FromResult<bool>(true);
            }
            catch (Exception ex)
            {
                GeneralContext.Logger.Error(ex.GetApiMessageInfo());
                return await Task.FromResult<bool>(false);
            }
        }

        /// <summary> Refresh cache by key, TODO: temp</summary>
        public async Task RefreshCacheAsync(string cacheKey = null)
        {
            //using var scope = GeneralContext.CreateServiceScope();
            if (GeneralContext.ServiceScope != null)
            {
                try
                {
                    var cacheItems = this.GetCacheItems<ApiCacheItem>();
                    if (cacheKey != null)
                        cacheItems = cacheItems.Where(x => x.Key == cacheKey).ToDictionary(k => k.Key, v => v.Value);

                    DbContext dbContext = GeneralContext.ServiceScope.ServiceProvider.GetService<DbContext>();

                    var items = cacheItems.Where(x => x.Value?.Query.ToUpper().Contains("SELECT") ?? false);
                    foreach (var item in items)
                    {
                        using var conn = dbContext.Database.GetDbConnection();

                        if (conn.State.Equals(ConnectionState.Closed))
                            conn.Open();
                        using var command = conn.CreateCommand();
                        command.CommandText = item.Value.Query;

                        //var result = command.ExecuteNonQuery();
                        //var result = dbContext.Database.ExecuteSqlRaw(item.Value.Query);

                        //var result = dbContext.Set<User>()
                        //    .FromSqlRaw<User>(item.Value.Query)
                        //    .ToList();

                        //var l = dbContext.User.Local;

                        //var list = dbContext.GetTrackEntries<User>();

                        using var db = new NpgsqlConnection(conn.ConnectionString);
                        //var result1 = db.Query<User>(item.Value.Query);

                        ////using var cmd = new NpgsqlCommand(item.Value.Query, con);
                        ////cmd.q

                        //var cacheItem = new ApiCacheItem(result, true, true, item.ToString());
                        //await _cache.SetAsync(item.Key, Util.ConvertObjectToByteArray(cacheItem));

                        //var encodedData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
                        //await _cache.SetAsync(cacheKey, encodedData, new DistributedCacheEntryOptions());

                        _logger.LogInformation("{cacheKey} cache refreshed", item.Key);
                    }
                }
                catch (System.Exception ex)
                {
                    GeneralContext.Logger.Error(ex.GetApiMessageInfo());
                }
            }
        }

        /// <summary> get maint model type <T> from Listt<T> </summary>
        public Type GetModelType<TResult>()
        {
            Type resultType;
            Type type = typeof(TResult);
            if (type.IsGenericType && type.GenericTypeArguments.Length == 1)
                resultType = type.GenericTypeArguments[0];
            else
                resultType = type;
            return resultType;
        }
        public Type GetModelType(Type type)
        {
            if (type == null) return type;

            Type modelType = null;
            if (type.IsGenericType && type.GenericTypeArguments.Length == 1)
            {
                if (type.BaseType == typeof(ObjectResult))
                {
                    var objectResultType = type.GenericTypeArguments.FirstOrDefault();
                    if (objectResultType.IsGenericType && objectResultType.GenericTypeArguments.Length == 1)
                        type = objectResultType;
                }
                modelType = type.GenericTypeArguments[0];
            }
            else
            {
                modelType = type;
            }

            return modelType;
        }
        public Type GetModelType(MethodInfo type)
        {
            return null;
        }

        #endregion public methods

        #region temp channel 

        public ValueTask<bool> WaitToReadAsync(CancellationToken stoppingToken = default)
        {
            _stoppingToken = stoppingToken;

            //if (IsCancellationRequested)
            //    return new ValueTask<bool>(false);

            return _channelReader.WaitToReadAsync(stoppingToken);
        }

        public ValueTask<ApiCacheItem> ReadAsync(CancellationToken stoppingToken = default)
        {
            return _channelReader.ReadAsync(stoppingToken);
        }

        public ValueTask WriteAsync(ApiCacheItem item, CancellationToken stoppingToken = default)
        {
            return _channelWriter.WriteAsync(item, stoppingToken);
        }

        #endregion channel

        #region temp implementation IDistibutedCache

        //public byte[] Get(string key)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public void Refresh(string key)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public Task RefreshAsync(string key, CancellationToken token = default)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public void Remove(string key)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public Task RemoveAsync(string key, CancellationToken token = default)
        //{
        //    throw new System.NotImplementedException();
        //}

        #endregion temp implementation IDistibutedCache
    }
}

