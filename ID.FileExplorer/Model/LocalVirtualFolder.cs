using FileExplorer.Shell;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileExplorer.Model
{
    public class LocalVirtualFolder : LocalFolder
    {
        public LocalVirtualFolder(string virtualPathName = "BaseVirtualFolder", string relativeFolderName = null) : base(string.Empty, null)
        {
            this.Name = virtualPathName;
            this.Parent = this;
            this.IsCheckVisible = false;
            this.SpecialFolderType = SpecialFolderType.ApplicationData;
            var path = relativeFolderName ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IntelliDesk");
            this.RealFolder = new LocalFolder(path);
        }

        public LocalVirtualFolder(DataSourceShell shellItem) : base(string.Empty, null)
        {
            if (shellItem.IsNull())
                throw new ArgumentNullException();
            this.Name = shellItem.DisplayName;
            this.Icon = shellItem.Icon;
            this.FullPath = shellItem.Path;
            this.IsCheckVisible = false;
            this.SpecialFolderType = shellItem.SpecialFolderType;
            this.Parent = this;
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IntelliDesk");
            this.RealFolder = new LocalFolder(path);
        }

        public SpecialFolderType SpecialFolderType { get; set; }

        private IFolder _realFolder;
        public IFolder RealFolder
        {
            get { return _realFolder; }
            set
            {
                if (_realFolder != value)
                {
                    if (!_realFolder.IsNull())
                    {
                        _realFolder.VirtualParent = null;
                    }

                    _realFolder = value;
                    if (!_realFolder.IsNull())
                    {
                        _realFolder.VirtualParent = this;
                    }
                }
            }
        }

        public override async Task<IFolder> GetFoldersAsync(bool isRecursive = false)
        {
            if (RealFolder.IsNull())
            {
            }

            await this.RealFolder.GetFoldersAsync();
            //folders = AddItemsByChunk(folders); //this.Folders, this.Items

            return this;
        }

        public override async Task<IFile> GetFilesAsync()
        {
            if (RealFolder.IsNull())
                return null;

            await this.RealFolder.GetFilesAsync();
            //files = AddItemsByChunk(files); //, this.Files, this.Items
            return this;
        }
    }
}