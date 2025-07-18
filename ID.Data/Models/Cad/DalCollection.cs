using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Intellidesk.Data.Models.Entities;

namespace Intellidesk.Data.Models.Cad
{
    public abstract class DalCollection<TEntity> : IDbSet<TEntity> where TEntity : BaseEntity
    {
        private readonly DbSet<TEntity> dbSet;

        protected DalCollection(DbSet<TEntity> dbSet)
        {
            this.dbSet = dbSet;
        }

        public ObservableCollection<TEntity> Local => dbSet.Local;

        public Type ElementType => ((IQueryable)dbSet).ElementType;

        public IQueryProvider Provider => ((IQueryable) dbSet).Provider;

        public Expression Expression => ((IQueryable)dbSet).Expression;

        public TEntity Add(TEntity entity)
        {
            return dbSet.Add(entity);
        }

        public TEntity Attach(TEntity entity)
        {
            return dbSet.Attach(entity);
        }

        public TEntity Create()
        {
            return dbSet.Create();
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity
        {
            return (TDerivedEntity)dbSet.Create();
        }

        public TEntity Remove(TEntity entity)
        {
            return dbSet.Remove(entity);
        }

        public TEntity Find(params object[] keyValues)
        {
            return dbSet.Find(keyValues);
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return ((IEnumerable<TEntity>)dbSet).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)dbSet).GetEnumerator();
        }

        public IEnumerable<TEntity> FindByDefault(Expression<Func<TEntity, bool>> exp)
        {
            if (Local.AsQueryable().Any(exp))
            {
                return Local.AsQueryable().Where(exp);
            }
            return this.Where(exp);
        }

        public TEntity FirstByDefault(Expression<Func<TEntity, bool>> exp)
        {
            if (Local.AsQueryable().Any(exp))
            {
                return Local.AsQueryable().FirstOrDefault(exp);
            }
            return this.FirstOrDefault(exp);
        }
    }
}