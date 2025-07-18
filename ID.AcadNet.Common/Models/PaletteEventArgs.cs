using System;

namespace Intellidesk.AcadNet.Common.Models
{
    public abstract class PaletteEventArgs : EventArgs
    {
        private string _paletteName;
        public PaletteEventArgs(string paletteName)
        {
            _paletteName = paletteName;
        }

        public string PaletteName
        {
            get { return _paletteName; }
        }
    }
}