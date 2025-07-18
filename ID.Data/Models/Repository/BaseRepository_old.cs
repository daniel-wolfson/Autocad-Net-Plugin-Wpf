using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

using AcadNet.Data.Repository;

namespace AcadNet.Data.Repository
{
    /// <summary> Test </summary>
    public class BaseRepository<T> : IGenericRepository<T> where T : class
    {
        protected DbSet<T> DbSet;

        public BaseRepository(DbContext context)
        {
            DbSet = context.Set<T>();
        }

        #region IRepository<T> Members

        public IEnumerable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate).AsEnumerable();
        }

        public void Add(T entity)
        {
            DbSet.Add(entity);
        }

        public void Delete(T entity)
        {
            DbSet.Remove(entity);
        }
        public void DeleteFor(Expression<Func<T, bool>> predicate)
        {
            Get(predicate).ToList().ForEach(ent => DbSet.Remove(ent));
        }

        public IQueryable<T> SearchFor(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate);
        }

        public IQueryable<T> GetAll()
        {
            return DbSet;
        }

        public T GetById(int id)
        {
            return DbSet.Find(id);
        }

        #endregion
    }

    /// <summary> Test </summary>
    //public class BaseRepository1<TEntity> : IRepository1<TEntity> where TEntity : class
    //{
    //    //private readonly IObjectContext _objectContext = null;
    //    private readonly IObjectSet<TEntity> _objectSet = null;

    //    //public BaseRepository(IObjectContext objectContext)
    //    //{
    //    //    if (objectContext == null)
    //    //        throw new ArgumentNullException("objectContext");

    //    //    _objectContext = objectContext;
    //    //    _objectSet = _objectContext.CreateObjectSet<TEntity>();
    //    //}

    //    protected IQueryable<TEntity> GetQuery()
    //    {
    //        return _objectSet;
    //    }

    //    public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> where)
    //    {
    //        return _objectSet.Where(where);
    //    }

    //    //public IEnumerable<TEntity> Find(Specification<TEntity> specification)
    //    //{
    //    //    return specification.SatisfyingElementsFrom(GetQuery());
    //    //}

    //    //public TEntity Single(Specification<TEntity> specification)
    //    //{
    //    //    return _objectSet.SingleOrDefault(specification.MatchingCriteria);
    //    //}

    //    public TEntity Single(Expression<Func<TEntity, bool>> where)
    //    {
    //        return _objectSet.SingleOrDefault(where);
    //    }

    //    public void Delete(TEntity entity)
    //    {
    //        _objectSet.DeleteObject(entity);
    //    }

    //    public void Add(TEntity entity)
    //    {
    //        _objectSet.AddObject(entity);
    //    }

    //    public IEnumerable<TEntity> Find(Func<TEntity, bool> where)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public TEntity Single(Func<TEntity, bool> where)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}