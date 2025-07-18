using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using General.Models;
using General.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace General.Interfaces
{
    public interface IBaseService
    {
        IDBContext DbContext { get; set; }
        HttpMethod CurrentHttpMethod { get; }

        // Transactions
        QueryTrackingBehavior QueryTrackingBehavior { get; set; }
        IDbContextTransaction Transaction { get; }
        bool IsTransactionEnabled { get; set; }
        bool IsOwnerTransaction { get; }
        bool IsOwnerDbContext { get; }
        CancellationToken CancelToken { get; set; }
        int Commit();
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
        bool Rollback();

        // Parent/Childs
        IBaseService Parent { get; set; }
        ConcurrentDictionary<string, IBaseService> ChildServices { get; }
        TService CreateChildService<TService>() where TService : IBaseService;
        TRepository CreateChildRepository<TRepository>() where TRepository : IBaseRepository;
        GenericRepository<TEntity, TKey> CreateChildGenericRepository<TEntity, TKey>() where TEntity : class where TKey : struct;

        // Results
        Task<ServiceObjectResult<bool>> OkResult();
        Task<ServiceObjectResult<bool>> OkResult(List<EntityEntry> operationResults);
        Task<ServiceObjectResult<TEntity>> OkResult<TEntity>(EntityEntry<TEntity> result) where TEntity : class;
        Task<ServiceObjectResult<bool>> OkResult(List<bool> operationResults);
        Task<ServiceObjectResult<T>> ServiceResult<T>(T value, HttpStatusCode statusCode = HttpStatusCode.OK, string message = null, T defaultValue = default);

        Task<ServiceObjectResult<TResult>> ErrorResult<TResult>(Exception ex, TResult defaultValue = default);
        Task<ServiceObjectResult<TResult>> ErrorResult<TResult>(string message, TResult defaultValue = default);
        Task<ServiceObjectResult<bool>> ErrorResult(Exception ex, bool defaultValue = default);

    }
}