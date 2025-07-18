using System.Collections.Generic;
using System.ComponentModel;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface ICollectionView<T> : IEnumerable<T>, ICollectionView
    {
        IEnumerable<T> SourceCollectionGeneric { get; }
    }
}