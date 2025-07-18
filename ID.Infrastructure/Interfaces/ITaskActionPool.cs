using System;
using System.Collections.Generic;

namespace ID.Infrastructure.Interfaces
{
    /// <summary> ITaskArgs </summary>
    public interface ITaskActionPool
    {
        /// <summary> Pool of actions to work with DB  </summary>
        IDictionary<string, Func<ITaskArgs, bool>> ActionPool { get; set; }
    }
}