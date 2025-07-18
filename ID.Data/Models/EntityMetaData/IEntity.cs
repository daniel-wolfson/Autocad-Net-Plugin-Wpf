using System;

namespace Intellidesk.Data.Models.EntityMetaData
{
    public interface IEntity
    {
        event EventHandler<EntityChangedArgs> EntityChangedEvent;
        bool IsModified();
        bool IsValid();
    }

    public interface IEntity<T>
    {
        T Id { get; set; }
        //event EventHandler<EntityChangedArgs> EntityChangedEvent;
    }
}