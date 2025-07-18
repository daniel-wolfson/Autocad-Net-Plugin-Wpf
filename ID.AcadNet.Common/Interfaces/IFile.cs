using ID.Infrastucture.Enums;
using Intellidesk.Data.Models.EntityMetaData;
using Intellidesk.Data.Repositories.Infrastructure;
using System;
using System.Windows.Media;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IFile : ICloneable, IObjectState, IEntity
    {
        /// <summary>
        /// Full file path
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// File name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// File extension 
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// File title for display
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Type name
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Folder path for search
        /// </summary>
        string FolderPath { get; }

        DateTime LastModifyTime { get; }
        string LastModifyTimeString { get; }

        long Size { get; }
        string SizeString { get; }

        ImageSource Icon { get; set; }

        bool IsEnabled { get; set; }

        /// <summary>
        /// IsFolder for sort ordering
        /// </summary>
        bool IsFolder { get; }

        /// <summary> IsFile for sort ordering </summary>
        bool IsFile { get; }

        bool IsFileShortcut { get; }

        /// <summary>
        /// True: full checked
        /// Null: half checked
        /// False: not checked
        /// </summary>
        bool? IsChecked { get; set; }

        void SetChecked(bool? isChecked);

        IFolder Parent { get; }

        FileStatus Status { get; set; }

        IFileDataInfoBase DataInfo { get; set; }
    }
}
