using System;
using System.Collections.Generic;
using System.Linq;

namespace Intellidesk.AcadNet.Common.Models
{
    public class BlockAttributeEqualityComparer : IEqualityComparer<BlockAttribute>
    {
        public bool Equals(BlockAttribute x, BlockAttribute y)
        {
            return
                x.Name.Equals(y.Name, StringComparison.CurrentCultureIgnoreCase) &&
                x.Attributes.SequenceEqual(y.Attributes);
        }

        public int GetHashCode(BlockAttribute obj)
        {
            return base.GetHashCode();
        }
    }
}