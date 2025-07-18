using System.Threading;
using System.Windows;
using System.Windows.Documents;
using Intellidesk.AcadNet.Common.Interfaces;

namespace Intellidesk.AcadNet.Core
{
    public class RunFolder: Run
    {
        static int _counter = 0;
        public RunFolder(IFolder folder, Style style) : base(folder.Name)
        {
            Interlocked.Increment(ref _counter);
            Name = "folder_" + _counter;
            Style = style;
            LinkedFolder = folder;
            ToolTip = folder.FullPath;
        }

        public RunFolder(string linkName, Style style = null) : base(linkName)
        {
            Interlocked.Increment(ref _counter);
            Name = "folder_" + _counter;
            Style = style;
            LinkedFolder = null;
            ToolTip = null;
        }

        public IFolder LinkedFolder { get; set; }
    }
}
