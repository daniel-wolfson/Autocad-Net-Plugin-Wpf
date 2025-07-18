using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AcadNet.Data.Models;

namespace AcadNet.Data.Repository
{
    public abstract class BaseRepository<T> : IRepository<T>
       where T : BaseEntity
    {
        protected DbContext _entities;
        protected readonly IDbSet<T> _dbset;

        public BaseRepository(DbContext context)
        {
            _entities = context;
            _dbset = context.Set<T>();
        }

        public virtual IEnumerable<T> GetAll()
        {
            return _dbset.AsEnumerable<T>();
        }

        public IEnumerable<T> FindBy(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            IEnumerable<T> query = _dbset.Where(predicate).AsEnumerable();
            return query;
        }

        public virtual T Add(T entity)
        {
            return _dbset.Add(entity);
        }

        public virtual T Delete(T entity)
        {
            return _dbset.Remove(entity);
        }

        public virtual void Edit(T entity)
        {
            _entities.Entry(entity).State = System.Data.Entity.EntityState.Modified;
        }

        public virtual void Save()
        {
            _entities.SaveChanges();
        }

        public void Update(T entity, string propertyName, object value)
        {
            _entities.Configuration.ValidateOnSaveEnabled = false;
            if (_entities.Entry(entity).State == EntityState.Detached)
                _entities.Set<T>().Attach(entity);

            entity.GetType().GetProperty(propertyName).SetValue(entity, value, null);
            _entities.Entry(entity).Property(propertyName).IsModified = true;

            //_entities.Entry(entity).State = EntityState.Modified;
        }

        public void Update(T entity, params string[] properties)
        {
            _entities.Configuration.ValidateOnSaveEnabled = false;
            if (_entities.Entry(entity).State == EntityState.Detached)
                _entities.Set<T>().Attach(entity);

            if (properties.Length > 0)
                foreach (var property in properties)
                    _entities.Entry(entity).Property(property).IsModified = true;

        }

        //public void Dispose()
        //{
        //    if (_entities != null)
        //    {
        //        _entities.SaveChanges();
        //        (_entities as IDisposable).Dispose();
        //        _entities = null;
        //    }
        //}
    }
}
