using ID.Infrastructure.DataContext;
using ID.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ID.Infrastructure.Interfaces
{
    public interface IBaseService : IUnitOfWork<IDbContext>
    {
        IDbContext DbContext { get; set; }
        HttpMethod CurrentHttpMethod { get; }

        // Transactions
        QueryTrackingBehavior QueryTrackingBehavior { get; set; }
        IDbContextTransaction Transaction { get; }
        bool? IsTransactionEnabled { get; set; }
        bool IsTransactionOwner { get; }
        bool IsDbContextOwner { get; }
        CancellationToken CancelToken { get; set; }

        // Parent/Childs
        IBaseService Parent { get; set; }
        TService GetChildService<TService>() where TService : IBaseService;
        TRepository GetChildRepository<TRepository>() where TRepository : IBaseRepository;

        // Results
        Task<ServiceResult<T>> OkResult<T>(T value, HttpStatusCode statusCode = HttpStatusCode.OK, string message = null);
        Task<ServiceResult<bool>> OkResult();
        Task<ServiceResult<bool>> OkResult(List<EntityEntry> operationResults);
        Task<ServiceResult<TEntity>> OkResult<TEntity>(EntityEntry<TEntity> result) where TEntity : class;
        Task<ServiceResult<bool>> OkResult(List<bool> operationResults);
        Task<ServiceResult<TResult>> ErrorResult<TResult>(Exception ex);
        Task<ServiceResult<bool>> ErrorResult(Exception ex);
        Task<ServiceResult<bool>> ErrorResult(string message);
    }
}
