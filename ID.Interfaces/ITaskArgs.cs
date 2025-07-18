using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using Intellidesk.Common.Enums;

namespace Intellidesk.Interfaces
{
    /// <summary> ITaskArgs </summary>
    public interface ITaskArgs
    {
        /// <summary> Action for worker successed  </summary>
        Action<object> ActionCompleted { get; set; }

        /// <summary> Action result </summary>
        object ActionResult { get; set; }

        /// <summary> Batch Size for partial operations </summary>
        int BatchSize { get; set; }

        /// <summary> Is Check mode as ON/OFF </summary>
        bool IsVerifyCheckBox { get; set; }

        /// <summary> Text content of task dialog </summary>
        string Content { get; set; }

        /// <summary> Current DbContext context </summary>
        DbContext Context { get; set; }

        /// <summary> List command's arguments </summary>
        IList CommandParameters { get; set; }

        /// <summary> Text command </summary>
        string Command { get; set; }

        /// <summary> An object that is used as a source </summary>
        object DataSource { get; set; }

        /// <summary> ExpandedInfo text of task dialog </summary>
        string ExpandedInfo { get; set; }

        /// <summary> ExpandedInfo text of task dialog 
        /// <example> taskArgs.ErrorInfo.Add("...");</example>/// </summary>
        Dictionary<NotifyStatus, string> CommandInfo { get; set; }

        /// <summary> true if timerCallBack is On </summary>
        bool IsTimerOn { get; set; }

        /// <summary> Title </summary>
        int ProgressPercentage { get; set; }

        /// <summary> Progress step </summary>
        int ProgressStep { get; set; }

        /// <summary> Progress bar max value </summary>
        int ProgressLimit { get; set; }

        /// <summary> Current progress index </summary>
        int ProgressIndex { get; set; }

        /// <summary> ProgressBar object </summary>
        object ProgressBar { get; set; }

        /// <summary> Current status </summary>
        StatusOptions Status { get; set; }

        /// <summary> Task title </summary>
        string Title { get; set; }

        /// <summary> TaskResult object  </summary>
        object TaskResult { get; set; }

        /// <summary> Reset </summary>
        void Reset();

        //bool DisplayStatus(uint timerTickCount);
        //Thread TaskThread { get; set; }
        //BackgroundWorker Worker { get; set; }
    }
}