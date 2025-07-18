using AutoMapper;
using ID.Infrastructure.Core;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ID.Infrastructure.Services
{
    public abstract class BaseService : IBaseService, IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource(); // Create the token source.
        private readonly Lazy<GeneralHttpClient> _CalcClient = new Lazy<GeneralHttpClient>(() => GeneralContext.CreateRestClient(ApiServiceNames.CalcApi));

        /// <summary> Current Calculator http rest client for access to CalculatorApi </summary>
        public GeneralHttpClient CalcApi => _CalcClient.Value;

        #region Public properties

        /// <summary> Is owner who created DbContext </summary>
        public bool IsDbContextOwner { get; private set; }
        /// <summary> Is owner who created DbContext </summary>
        public bool IsTransactionOwner { get; private set; }

        public bool IsAutoSaveChangesEnabled { get; private set; }

        private bool? _isTransactionEnabled;
        public bool? IsTransactionEnabled
        {
            get
            {
                if (!_isTransactionEnabled.HasValue)
                {
                    _isTransactionEnabled = CurrentHttpMethod != HttpMethod.Get;
                    IsAutoSaveChangesEnabled = _isTransactionEnabled.GetValueOrDefault();
                }
                return _isTransactionEnabled;
            }

            set
            {
                _isTransactionEnabled = value;
                IsAutoSaveChangesEnabled = _isTransactionEnabled.GetValueOrDefault();
            }
        }

        //public bool UseNewContext { get; set; } = false;

        private IBaseService _parent;
        public virtual IBaseService Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                this.DbContext = value.DbContext;
                this.QueryTrackingBehavior = value.QueryTrackingBehavior;
                this.IsTransactionEnabled = value.IsTransactionEnabled;
            }
        }

        /// <summary> Action's cancellation token </summary>
        public CancellationToken CancelToken { get; set; } = default;

        /// <summary> Get current transaction from Database </summary>
        public IDbContextTransaction Transaction => DbContext?.Database?.CurrentTransaction;

        /// <summary> Request absolute path of sender </summary>
        public string RequestUrl
        {
            get
            {
                var urlHelperFactory = GeneralContext.GetService<IUrlHelperFactory>();
                var actionContextAccessor = GeneralContext.GetService<IActionContextAccessor>();
                if (actionContextAccessor.ActionContext != null)
                {
                    var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
                    return urlHelper.GetAbsoluteUrl();
                }
                else
                {
                    return HttpContext.Request.Path;
                }
            }
        }

        public HttpContext HttpContext => GeneralContext.HttpContext;

        public IAuthOptions AuthOptions => GeneralContext.GetService<IAuthOptions>();

        /// <summary> Current HttpMethod on create service</summary>
        public HttpMethod CurrentHttpMethod
        {
            get
            {
                if (string.IsNullOrEmpty(HttpContext.Request.Method))
                    return HttpMethod.Get;
                return new HttpMethod(HttpContext.Request.Method);
            }
        }

        public bool IsAjaxRequest => HttpContext.Request.Headers["x-requested-with"] == "XMLHttpRequest";

        /// <summary> The change tracker will not track any of the entities that are returned from a LINQ query /// </summary>
        private QueryTrackingBehavior _queryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        public QueryTrackingBehavior QueryTrackingBehavior
        {
            get { return _queryTrackingBehavior; }
            set
            {
                _queryTrackingBehavior = value;
                _dbContext.ChangeTracker.QueryTrackingBehavior = _queryTrackingBehavior;
            }
        }

        public IDbContext _dbContext = null;
        public IDbContext DbContext
        {
            get
            {
                if (_dbContext == null)
                {
                    _dbContext = GeneralContext.GetService<IDbContext>();

                    var _isTransactionEnabled = IsTransactionEnabled.GetValueOrDefault();

                    QueryTrackingBehavior = _isTransactionEnabled
                        ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking;

                    if (_isTransactionEnabled)
                        ForceBeginTransaction();

                    IsDbContextOwner = true;
                }
                return _dbContext;
            }
            set
            {
                _dbContext = value;
            }
        }

        public IMapper Mapper => GeneralContext.GetService<IMapper>();

        #endregion Public properties

        #region Create child services

        /// <summary> Create child service and set this as parent </summary>
        public virtual TService GetChildService<TService>() where TService : IBaseService
        {
            TService service = default;
            try
            {
                service = HttpContext.RequestServices.GetRequiredService<TService>();
                service.Parent = this;
                service.IsTransactionEnabled = this.IsTransactionEnabled;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Serice {typeof(TService).Name} not registred {ex.GetApiMessageInfo()}";
                throw new InvalidOperationException(GeneralContext.GetApiMessageInfo(errorMessage, EventLevel.Critical));
            }
            return service;
        }

        /// <summary> Create child repository and set this as parent </summary>
        public virtual TRepository GetChildRepository<TRepository>() where TRepository : IBaseRepository
        {
            TRepository repo = HttpContext.RequestServices.GetRequiredService<TRepository>();
            if (repo != null)
            {
                repo.Parent = this;
            }
            else
            {
                var errorMessage = $"Repository {typeof(TRepository).Name} not registred";
                throw new InvalidOperationException(GeneralContext.GetApiMessageInfo(errorMessage, EventLevel.Critical));
            }
            return repo;
        }

        /// <summary> Create service in root scope </summary>
        public TService GetRootService<TService>() where TService : BaseService
        {
            var _serviceScope = GeneralContext.CreateServiceScope();
            var service = GeneralContext.GetService<TService>();
            service.DbContext = _serviceScope.ServiceProvider.GetService<IDbContext>();
            return service as TService;
        }

        public TService GetChildScopeService<TService>() where TService : BaseService
        {
            var _serviceScope = GeneralContext.CreateServiceScope();
            var service = _serviceScope.ServiceProvider.GetService<TService>();
            service.DbContext = _serviceScope.ServiceProvider.GetService<IDbContext>();
            return service as TService;
        }

        #endregion

        #region SaveChanges/Commit/Rollback

        /// <summary> SaveChanges </summary>
        /// <returns>The number of state entries written to the database</returns>
        public virtual int SaveChanges()
        {
            var writtenCount = DbContext.SaveChanges();
            return writtenCount;
        }

        /// <summary> SaveChanges </summary>
        /// <returns>The number of state entries written to the database</returns>
        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var writtenCount = await DbContext.SaveChangesAsync();
            return writtenCount;
        }

        #endregion SaveChanges/Commit/Rollback

        #region Ok/Error results

        /// <summary> Commit transaction, if IsTransactionEnabledEnabled=true, and try return HttpStatusCode.OK </summary>
        public Task<ServiceResult<bool>> OkResult()
        {
            return OkResult(true, HttpStatusCode.OK);
        }
        /// <summary> Commit transaction, if IsTransactionEnabledEnabled=true, and try return HttpStatusCode.OK </summary>
        public Task<ServiceResult<T>> OkResult<T>(T value, HttpStatusCode statusCode = HttpStatusCode.OK, string message = null)
        {
            if (IsTransactionEnabled.GetValueOrDefault() && statusCode == HttpStatusCode.OK)
            {
                var committedChangesCount = this.CommitTransaction();
                message = $"{message ?? ""} {committedChangesCount} savechanges committed";
                statusCode = committedChangesCount > 0 ? HttpStatusCode.OK : HttpStatusCode.NotModified;

                if (typeof(T) == typeof(HttpStatusCode))
                {
                    statusCode = HttpStatusCode.OK;
                    message += "; value contains statusCode";
                }
            }

            var result = new ServiceResult<T>(value, statusCode, message) { RequestUrl = RequestUrl };
            return Task.FromResult(result);
        }
        /// <summary> Commit transaction, if IsTransactionEnabledEnabled=true, and try return HttpStatusCode.OK </summary>
        public Task<ServiceResult<TEntity>> OkResult<TEntity>(EntityEntry<TEntity> result) where TEntity : class
        {
            HttpStatusCode statusCode = result.State != EntityState.Unchanged ? HttpStatusCode.OK : HttpStatusCode.NotModified;
            return OkResult(result.Entity, statusCode, $"state: {result.State}; statusCode: {statusCode}");
        }
        /// <summary> Commit transaction, if IsTransactionEnabledEnabled=true, and try return HttpStatusCode.OK </summary>
        public Task<ServiceResult<bool>> OkResult(List<EntityEntry> operationResults)
        {
            var summarySuccess = operationResults.All(result => result.State != EntityState.Unchanged);
            string summaryMessage = string.Join("; ", operationResults.Select(result => result.ToString()));

            HttpStatusCode statusCode = summarySuccess ? HttpStatusCode.OK : HttpStatusCode.NotModified;
            return OkResult(summarySuccess, statusCode, summaryMessage);
        }
        /// <summary> Commit transaction, if IsTransactionEnabledEnabled=true, and try return HttpStatusCode.OK </summary>
        public Task<ServiceResult<bool>> OkResult(List<bool> operationResults)
        {
            var summarySuccess = operationResults.All(result => true);
            string summaryMessage = string.Join("; ", operationResults.Select(result => result.ToString()));

            HttpStatusCode statusCode = summarySuccess ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            return OkResult(summarySuccess, statusCode, summaryMessage);
        }

        /// <summary> Rollback transaction, and try return HttpStatusCode.Conflict </summary>
        public Task<ServiceResult<TResult>> ErrorResult<TResult>(Exception ex)
        {
            var result = new ServiceResult<TResult>(default(TResult), HttpStatusCode.Conflict, ex?.InnerException?.Message ?? ex.Message) { RequestUrl = RequestUrl };
            this.RollbackTransaction();
            return OkResult(default(TResult), HttpStatusCode.Conflict, result.Message);
        }
        /// <summary> Rollback transaction, and try return HttpStatusCode.Conflict </summary>
        public Task<ServiceResult<TResult>> ErrorResult<TResult>(string message = "operation error")
        {
            var result = new ServiceResult<TResult>(default(TResult), HttpStatusCode.Conflict, message) { RequestUrl = RequestUrl };
            this.RollbackTransaction();
            return OkResult(default(TResult), HttpStatusCode.Conflict, result.Message);
        }
        /// <summary> Rollback transaction, and try return HttpStatusCode.Conflict </summary>
        public Task<ServiceResult<bool>> ErrorResult(Exception ex)
        {
            return ErrorResult(ex?.InnerException?.Message ?? ex.Message);
        }
        /// <summary> Rollback transaction, and try return HttpStatusCode.Conflict </summary>
        public Task<ServiceResult<bool>> ErrorResult(string message)
        {
            this.RollbackTransaction();
            return OkResult(false, HttpStatusCode.Conflict, message);
        }

        /// <summary> EntityResult, by state of entityState </summary>
        public ServiceResult<bool> EntityResult<TEntity>(Task<EntityEntry<TEntity>> task) where TEntity : class
        {
            //object value = typeof(TResult) == typeof(bool) 
            //    ? Convert.ChangeType(task.Result.State != EntityState.Unchanged, typeof(TResult)) 
            //    : task.Result.Entity;

            //TResult result = (TResult)Convert.ChangeType(value, typeof(TResult));

            return new ServiceResult<bool>(task.Result.State != EntityState.Unchanged,
                task.Result.State != EntityState.Unchanged ? HttpStatusCode.OK : HttpStatusCode.NotModified);
        }

        #endregion Ok/Error results

        #region Helpers (ChildServices, ChildRepositories)

        //TODO: remove SessionId
        public string SessionId => throw new NotImplementedException();

        public string GetConnectionString()
        {
            string connectionString = GeneralContext.GetConnectionString("CRPMDatabase");

            if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("{0}"))
                GeneralContext.Logger.Error("connectionString is null or empty or not valid");

            return connectionString;
        }

        //private bool isTransactionEnabled;
        private static readonly object lockingObject = new object();

        #endregion Helpers (ChildServices, ChildRepositories)

        #region IUnitOfWork

        public void ResetDbContext()
        {
            _dbContext = null;
        }

        // TODO: Implement
        public DbSet<T> Set<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public virtual void ForceBeginTransaction()
        {
            lock (lockingObject)
            {
                if (_dbContext.Database.CurrentTransaction == null)
                {
                    var _transaction = _dbContext.Database.BeginTransaction();
                    IsTransactionOwner = _transaction != null;
                    CancelToken = _cts.Token;
                }
            }
        }

        public virtual int CommitTransaction()
        {
            string transactionId = "";
            int changesCount = 0;
            try
            {
                changesCount = DbContext.GetChangesCounter();
                if (changesCount == 0 && IsAutoSaveChangesEnabled)
                    changesCount = DbContext.SaveChanges();

                if (_dbContext.Database.CurrentTransaction != null && !CancelToken.IsCancellationRequested)
                {
                    transactionId = _dbContext.Database.CurrentTransaction.TransactionId.ToString();
                    _dbContext.Database.CurrentTransaction.Commit();
                    GeneralContext.Logger.Information($"transaction {transactionId} committed");
                }

                _dbContext.ClearChangesCounter();
            }
            catch (Exception ex)
            {
                GeneralContext.Logger.Error($"transaction {transactionId} not committed, because: \n{(ex.InnerException ?? ex).Message}");

                if (_dbContext.Database.CurrentTransaction != null)
                    _dbContext.Database.CurrentTransaction.Rollback();
                _cts.Cancel();
            }

            return changesCount;
        }

        public virtual async Task<int> CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            string transactionId = "";
            int changesCount = 0;
            try
            {
                changesCount = DbContext.GetChangesCounter();
                if (changesCount == 0 && IsAutoSaveChangesEnabled)
                    changesCount = await DbContext.SaveChangesAsync();

                if (_dbContext.Database.CurrentTransaction != null && !CancelToken.IsCancellationRequested)
                {
                    transactionId = _dbContext.Database.CurrentTransaction.TransactionId.ToString();
                    await _dbContext.Database.CurrentTransaction.CommitAsync();
                    GeneralContext.Logger.Information($"transaction {transactionId} committed");
                }

                _dbContext.ClearChangesCounter();
            }
            catch (Exception ex)
            {
                GeneralContext.Logger.Error($"transaction {transactionId} not committed, because: \n{(ex.InnerException ?? ex).Message}");

                if (_dbContext.Database.CurrentTransaction != null)
                    await _dbContext.Database.CurrentTransaction.RollbackAsync();

                _cts.Cancel();
            }
            return changesCount;
        }

        public virtual void RollbackTransaction()
        {
            if (_dbContext?.Database?.CurrentTransaction != null)
            {
                _dbContext.Database.CurrentTransaction.Rollback();
                //_dbContext.Database.CurrentTransaction.Dispose();
                _dbContext.Dispose();
                _dbContext = null;
            }
        }

        public virtual void SetIsolationLevel(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        public int CommitTransaction(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion IUnitOfWork

        #region Idisposable

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    //_serviceScope?.Dispose();
                    //try
                    //{
                    //    if (_dbContext?.Database?.CurrentTransaction != null && IsTransactionEnabled && !committed)
                    //    {
                    //        var transactionId = _dbContext?.Database?.CurrentTransaction?.TransactionId;
                    //        GeneralContext.Logger.Warning($"transactionId {transactionId} not committed");
                    //        this.RollbackTransaction();
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    GeneralContext.Logger.Error(ex, $"transaction committing");
                    //}
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BaseService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion Idisposable
    }
}
