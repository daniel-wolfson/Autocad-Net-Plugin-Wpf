using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading;
using Intellidesk.Common.Enums;

namespace Intellidesk.Interfaces
{
    public interface ITaskArguments
    {
        object ActionResult { get; set; }
        //IDictionary<string, Func<ITaskArgs, bool>> ActionPool { get; set; }
        Action<object> ActionCompleted { get; set; }
        int BatchSize { get; set; }
        CancellationToken CancellationToken { get; set; }
        bool IsCanceled { get; set; }
        string Command { get; set; }
        IList CommandParameters { get; set; }
        Dictionary<NotifyStatus, string> CommandInfo { get; set; }
        string Content { get; set; }
        DbContext Context { get; set; }
        object DataSource { get; set; }
        string DisplayName { get; set; }
        //IActiveTaskDialog Dialog { get; set; }
        object Dialog { get; set; }
        string ExpandedInfo { get; set; }
        bool IsVerifyCheckBox { get; set; }
        string Title { get; set; }
        bool IsTimerOn { get; set; }
        int ProgressPercentage { get; set; }
        int ProgressStep { get; set; }
        int ProgressLimit { get; set; }
        int ProgressIndex { get; set; }
        object ProgressBar { get; set; }
        StatusOptions Status { get; set; }
        object TaskResult { get; set; }

        ///// <summary> show status in dialog </summary>
        ///// <param name="timerTickCount">get timerTick from ActiveTaskDialog for display timer</param>
        bool DisplayStatus(uint timerTickCount);

        void Reset();
        void Dispose();
    }
}