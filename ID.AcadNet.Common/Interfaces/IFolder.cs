using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IFolder : IFile, IDisposable
    {
        /// <summary> is loading children  </summary>
        bool IsLoading { get; }

        /// <summary> can reloading  </summary>
        bool CanReloading();

        /// <summary> Is extracting canceled </summary>
        bool IsCanceled { get; }

        /// <summary> Is tree item  expanded </summary>
        bool IsExpanded { get; set; }

        /// <summary> Is tree item  selected </summary>
        bool IsSelected { get; set; }
        
        /// <summary> Is tree item  checkbox visible </summary>
        bool IsCheckVisible { get; set; }

        /// <summary> Is children folders loaded </summary>
        bool IsFolderLoaded { get; set; }

        /// <summary> Is children _files loaded </summary>
        bool IsFileLoaded { get; set; }

        /// <summary> Folder in virtual folder has a virtual parent </summary>
        IFolder VirtualParent { get; set; }

        /// <summary> Sub folders </summary>
        ObservableCollection<IFolder> Folders { get; }

        /// <summary> Sub files </summary>
        ObservableCollection<IFile> Files { get; }

        /// <summary> Sub folders and files </summary>
        ObservableCollection<IFile> Items { get; }

        /// <summary> Cancel current extracting </summary>
        void Cancel();

        /// <summary> Get sub folders and files async </summary>
        Task<IFolder> LoadChildrenAsync(bool isRecursive = false);

        Task<IFolder> GetChildrenAsync(bool isRecursive = false);

        /// <summary> Get sub folders async </summary>
        Task<IFolder> GetFoldersAsync(bool isRecursive = false);

        /// <summary> Get sub folders async </summary>
        Task<IFolder> SetFoldersAsync(string[] folderPaths);

        /// <summary> Get sub  files async </summary>
        Task<IFile> GetFilesAsync();

        /// <summary> Get sub foldr or file by path </summary>
        void GetItemAsync(string filePath, Action<IFile> callback, bool isRecursive = true);

        Task<IFolder> LoadFoldersByPathAsync(List<string> pathList = null, Action<IFolder> foundFolderCallBack = null);
    }
}
