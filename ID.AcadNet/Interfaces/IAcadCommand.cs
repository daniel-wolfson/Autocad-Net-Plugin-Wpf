using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IAcadCommand : ICommand
    {
        /// <summary> command </summary>
        object AcadCommand { get; set; }
        /// <summary> command arguments </summary>
        object AcadCommandArgs { get; set; }
    }
}
