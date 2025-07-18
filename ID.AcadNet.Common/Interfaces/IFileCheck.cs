using System.Collections.Generic;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IFileCheck
    {
        /// <summary> Checked item not to do the recursive  checked opertion </summary>
        void SetChecked(bool? isChecked);

        /// <summary> Checked the all parent for current item </summary>
        void CheckParent(IFile item, bool? isCheck);
    }

    public interface IFolderCheck : IFileCheck
    {   
        /// <summary> Checked the all children for current item </summary>
        void CheckChildren(IFolder item, bool? isChecked);

        /// <summary> Get all checked children for current item </summary>
        IEnumerable<IFile> GetCheckedItems(IFolder folder);
    }
}
