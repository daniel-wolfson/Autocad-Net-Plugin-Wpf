using System;

namespace Intellidesk.Data.Models.EntityMetaData
{
    public abstract class Entity<T> : IEntity<T>
    {
        public virtual T Id { get; set; }
        //public virtual event EventHandler<EntityChangedArgs> EntityChangedEvent;
    }
}