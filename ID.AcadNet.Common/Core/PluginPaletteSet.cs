using Autodesk.AutoCAD.Windows;
using ID.Infrastructure.Commands;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.Resources.Properties;
using System;
using System.Drawing;

namespace Intellidesk.AcadNet.Common.Core
{
    public delegate void PaletteExecuteStartEventhandler(object sender, PaletteExecuteEventArgs e);

    public delegate void PaletteExecuteCompleteEventhandler(object sender, PaletteExecuteEventArgs e);


    public class PluginPaletteSet : PaletteSet, IPluginPaletteSet
    {
        private static readonly string _paletteStaticName = CommandNames.UserGroup + ".Tools";
        private static readonly Guid? _paletteStaticGuid = new Guid(Settings.Default.PaletteSetId);

        private string _paletteName;
        public string PaletteName
        {
            get { return _paletteName; }
            set { _paletteName = value; }
        }

        private Guid _guid;
        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public PluginPaletteSet(string paletteName = null, Guid? guid = null) :
            base(string.IsNullOrEmpty(paletteName) ? _paletteStaticName : paletteName, (Guid)(guid ?? _paletteStaticGuid))
        {
            _paletteName = this.PaletteName;
            _guid = this.Guid;

            //Style = PaletteSetStyles.NameEditable | PaletteSetStyles.ShowPropertiesMenu |
            //            PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton,

            this.MinimumSize = new Size(300, 300);
            this.Size = new Size(ToolsManager.PluginSettings.ToolPanelWidth, 300);
            this.Style = PaletteSetStyles.ShowAutoHideButton |
                PaletteSetStyles.ShowCloseButton |
                PaletteSetStyles.ShowTabForSingle |
                PaletteSetStyles.Snappable;
            this.DockEnabled = (DockSides)((int)DockSides.Left + (int)DockSides.Right);
            this.Dock = DockSides.Left;
            this.DockEnabled = DockSides.None;

        }

        //public event PaletteExecuteStartEventhandler PaletteExecuteStarted;

        //public event PaletteExecuteCompleteEventhandler PaletteExecuteCompleted;
    }
}