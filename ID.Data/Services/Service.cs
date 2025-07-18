using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Intellidesk.AcadNet.Data.Repositories;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Models.EntityMetaData;
using Intellidesk.Data.Repositories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Intellidesk.Data.Repositories.Infrastructure;

namespace Intellidesk.Data.Services
{
    public abstract class Service<TEntity> : IService<TEntity> where TEntity : BaseEntity, IObjectState, new()
    {
        #region Private Fields
        protected readonly IRepository<TEntity> Repository;
        #endregion Private Fields

        #region Constructor
        protected Service(string xmlFileName)
        {
            var t = typeof(XmlRepository<TEntity>);
            Repository = (IRepository<TEntity>)Activator.CreateInstance(t, new object[] { xmlFileName });
        }
        protected Service(IRepository<TEntity> repository) { Repository = repository; }
        protected Service(IUnitOfWorkAsync uow) { Repository = uow.RepositoryAsync<TEntity>(); }

        #endregion Constructor

        public virtual TEntity Find(params object[] keyValues) { return Repository.Find(keyValues); }

        public virtual IQueryable<TEntity> SelectQuery(string query, params object[] parameters) { return Repository.SelectQuery(query, parameters).AsQueryable(); }

        public virtual void Insert(TEntity entity) { Repository.Insert(entity); }

        public virtual void InsertRange(IEnumerable<TEntity> entities) { Repository.InsertRange(entities); }

        public virtual void InsertOrUpdateGraph(TEntity entity) { Repository.InsertOrUpdateGraph(entity); }

        public virtual void InsertGraphRange(IEnumerable<TEntity> entities) { Repository.InsertGraphRange(entities); }

        public virtual void Update(TEntity entity) { Repository.Update(entity); }

        public virtual void Delete(object id) { Repository.Delete(id); }

        public virtual void Delete(TEntity entity) { Repository.Delete(entity); }

        public IQueryFluent<TEntity> Query() { return Repository.Query(); }

        public virtual IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject) { return Repository.Query(queryObject); }

        public virtual IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query) { return Repository.Query(query); }

        //public virtual async Task<TEntity> FindAsync(params object[] keyValues) { return await _repository.FindAsync(keyValues); }

        //public virtual async Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues) { return await _repository.FindAsync(cancellationToken, keyValues); }

        //public virtual async Task<bool> DeleteAsync(params object[] keyValues) { return await DeleteAsync(CancellationToken.None, keyValues); }

        //IF 04/08/2014 - Before: return await DeleteAsync(cancellationToken, keyValues);
        //public virtual async Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues) { return await _repository.DeleteAsync(cancellationToken, keyValues); }

        public IQueryable<TEntity> Queryable() { return Repository.Queryable(); }

        public virtual ObservableCollection<TEntity> LoadItems()
        {
            return ((IXmlRepository<TEntity>)Repository).Load().ToItems();
        }

        public virtual TEntity CreateInstanceByDefault()
        {
            return new TEntity(); //).Default<TEntity>();
        }

        public virtual TEntity Clone(TEntity obj)
        {
            return new TEntity();
        }
    }
}