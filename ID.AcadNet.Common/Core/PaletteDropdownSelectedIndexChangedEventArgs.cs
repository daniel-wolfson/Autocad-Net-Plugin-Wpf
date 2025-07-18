using Intellidesk.AcadNet.Common.Models;

namespace Intellidesk.AcadNet.Common.Core
{
    public class PaletteDropdownSelectedIndexChangedEventArgs : PaletteEventArgs
    {
        private int _selectedIndex;
        private string _selectedText;

        public PaletteDropdownSelectedIndexChangedEventArgs(
            int selectedIndex, string selectedText, string paletteName)
            : base(paletteName)
        {
            _selectedIndex = selectedIndex;
            _selectedText = selectedText;
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
        }

        public string SelectedText
        {
            get { return _selectedText; }
        }
    }
}
