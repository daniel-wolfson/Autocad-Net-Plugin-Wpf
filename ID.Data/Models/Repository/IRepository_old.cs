using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AcadNet.Data.Repository
{
    public interface IRepository<TEntity>
    {
        void Insert(TEntity entity);
        void Delete(TEntity entity);
        IQueryable<TEntity> SearchFor(Expression<Func<TEntity, bool>> predicate);
        IQueryable<TEntity> GetAll();
        TEntity GetById(int id);
    }

    public interface IRepository1<T> where T : class
    {
        IEnumerable<T> Find(Func<T, bool> where);
        //IEnumerable<T> Find(Specification<T> specification);
        T Single(Func<T, bool> where);
        //T Single(Specification<T> specification);
        void Delete(T entity);
        void Add(T entity);
    }
}