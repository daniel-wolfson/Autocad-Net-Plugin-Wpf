using Intellidesk.Data.Models.Entities;
using System.Linq;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class ElementDefenitionExtensions
    {
        public static string[] XGetHandleItems(this IPaletteElement element)
        {
            return element.Items.Select(x => x).Concat(new[] { element.Handle }).ToArray();
        }
    }
}
