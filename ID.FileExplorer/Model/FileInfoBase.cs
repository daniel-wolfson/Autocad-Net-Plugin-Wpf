using System.ComponentModel;
using FileExplorer.ViewModel;
using Intellidesk.AcadNet.Common.Interfaces;

namespace FileExplorer.Model
{
    public class FileDataInfoBase : ViewModelBase, IFileDataInfoBase
    {
        private string _type = "undefined";
        [Category("Data"), Description("Data type"), ReadOnly(false)]
        public virtual string Type
        {
            get { return _type; }
            set { Set(ref _type, value); }
        }
    }
}