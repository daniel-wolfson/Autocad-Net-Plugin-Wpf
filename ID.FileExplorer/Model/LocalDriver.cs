using System.IO;
using Intellidesk.AcadNet.Common.Interfaces;

namespace FileExplorer.Model
{
    public class LocalDriver : LocalFolder
    {
        public LocalDriver(string path, IFolder parent)
            : base(path, parent)
        {
            /// CD-ROM is not existed 
            /// Virtual folder is not existed

            this.FullPath = path;
            this.Parent = parent;
            this.AddPlaceHolder();
            fileAttr = FileAttributes.Directory;
        }
    }
}