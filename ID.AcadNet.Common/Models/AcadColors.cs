using Intellidesk.Data.Models.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Color = System.Drawing.Color;

namespace Intellidesk.AcadNet.Common.Models
{
    public class AcadColors : ObservableCollection<AcadColor>
    {
        public bool Contains(short colorIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].ColorIndex == colorIndex)
                    return true;
            }
            return false;
        }

        public void AddColors<T>(T[] elementTypes) where T : IPaletteElement
        {
            foreach (var type in elementTypes)
            {
                Add(new AcadColor((short)type.TypeCode));
            }
        }

        public List<AcadColor> GetColors<T>() where T : IPaletteElement
        {
            return Items.Where(x => x.ElementType != null && x.ElementType.BaseType == typeof(T)).ToList();
        }

        public void AddDefaultColors()
        {
            Add(new AcadColor("ByDefault", Color.White));
            Add(new AcadColor("ByLayer", Color.White));
            Add(new AcadColor("ByBlock", Color.White));
            Add(new AcadColor(Color.Red));
            Add(new AcadColor(Color.Yellow));
            Add(new AcadColor(Color.Green));
            Add(new AcadColor(Color.Cyan));
            Add(new AcadColor(Color.Blue));
            Add(new AcadColor(Color.Magenta));
            Add(new AcadColor(Color.White));
            Add(new AcadColor("Select Colors...", Color.White));
        }
    }
}