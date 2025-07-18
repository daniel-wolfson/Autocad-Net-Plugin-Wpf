using ID.Infrastructure.Interfaces;
using System.ComponentModel;
using TaskDialogInterop.Interfaces;

namespace Intellidesk.Data.Tasks
{
    /// <summary> DoTaskEventArgs </summary>
    public class DoTaskEventArgs : DoWorkEventArgs
    {
        /// <summary> Title </summary>
        IActiveTaskDialog Dialog { get; set; }

        /// <summary> TaskArgs </summary>
        public ITaskArgs TaskArgs { get; set; }

        /// <summary>  </summary>
        /// <param name="argument"></param>
        public DoTaskEventArgs(object argument)
            : base(argument)
        {
        }
    }
}