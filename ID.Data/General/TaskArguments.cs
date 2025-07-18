using ID.Infrastructure;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Interfaces;
using Intellidesk.Common.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading;
using Unity;

namespace Intellidesk.Data.General
{
    /// <summary> Task arguments </summary>
    public class TaskArguments : ITaskArguments, ITaskArgs, ITaskActionPool, IDisposable
    {
        public IPluginSettings AppSettings
        {
            get { return GeneralContext.GetService<IPluginSettings>(); }
        }

        public object Sender { get; set; }

        private int _progressIndex;
        private int _progressStep = 1;
        private object _dataSource;
        private IUnityContainer _container;
        private string _displayName;

        public object ActionResult { get; set; }
        public IDictionary<string, Func<ITaskArgs, bool>> ActionPool { get; set; } // pool command to performing
        public Action<object> ActionCompleted { get; set; } // action that occur after worker complete event
        public int BatchSize { get; set; }
        public string Command { get; set; } // as Delete, Undelete, Purge, Copy
        public IList CommandParameters { get; set; }
        public string Content { get; set; }
        public DbContext Context { get; set; }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_displayName))
                    return Command;
                return _displayName;
            }
            set { _displayName = value; }
        }

        public object DataSource
        {
            get { return _dataSource; }
            set
            {
                if (value != null) ProgressLimit = ((IList)value).Count;
                _dataSource = value;
            }
        }

        public string ExpandedInfo { get; set; }

        public TaskArguments()
        {
            BatchSize = 100;
        }

        public TaskArguments(IUnityContainer container)
        {
            _container = container;
            BatchSize = 100;
        }

        /// <summary> CommandInfo </summary>
        public Dictionary<NotifyStatus, string> CommandInfo { get; set; } =
            new Dictionary<NotifyStatus, string>();

        public bool IsVerifyCheckBox { get; set; }

        public string Title { get; set; }

        public bool IsTimerOn { get; set; }

        public int ProgressPercentage { get; set; }

        public int ProgressStep
        {
            get { return _progressStep; }
            set { _progressStep = value; }
        }

        public int ProgressLimit { get; set; }
        public int ProgressIndex
        {
            get { return _progressIndex; }
            set
            {
                _progressIndex = value;
                if (ProgressLimit == value)
                    ProgressPercentage = 100;
                else
                    ProgressPercentage = (int)((float)value / ProgressLimit * 100); //(int)((float)taskArgs.ProgressIndex / objectIds.Count * 100);
            }
        }

        public object ProgressBar { get; set; }

        public StatusOptions Status { get; set; } //current status of task such as StatusOptions:...

        public object TaskResult { get; set; }

        public CancellationToken CancellationToken { get; set; }

        private readonly object CancelLock = new object();
        private bool _isCanceled;
        public bool IsCanceled
        {
            get { lock (CancelLock) { return _isCanceled; } }
            set { _isCanceled = value; }
        }

        //public Thread TaskThread { get; set; }
        //public BackgroundWorker Worker { get; set; }

        public virtual object Dialog { get; set; }

        public void Reset()
        {
            Title = Content = ExpandedInfo = Command = "";
            ProgressIndex = 0;
            ProgressStep = 1;
            ProgressPercentage = 0;
            //Dialog = null;
            ActionResult = null;
            Status = StatusOptions.None;
            //worker = null;
            //taskThread = null;
            CommandParameters = null;
            ActionPool = null;
            TaskResult = null;
        }

        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
        }

        public bool DisplayStatus(uint timerTickCount)
        {
            return false;
        }
    }
}