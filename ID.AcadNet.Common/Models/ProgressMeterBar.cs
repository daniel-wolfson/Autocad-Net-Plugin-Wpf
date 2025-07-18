using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure.Helpers;
using Serilog;
using System;

namespace Intellidesk.AcadNet.Common.Models
{
    /// <summary> ProgressMeterBar </summary>
    public class ProgressMeterBar : ProgressMeter
    {
        public bool IsTimerInclude;
        public string DisplayString { get; set; }
        private static Util.Stopwatcher _watch;

        private int _progressLimit;
        public int ProgressLimit
        {
            get { return _progressLimit; }
            set
            {
                _progressLimit = value;
                SetLimit(value);
            }
        }

        public override void Start(String displayString = "")
        {
            Log.Logger.Information($"{nameof(ProgressMeterBar)}.{nameof(Start)} Progress started");
            //Log.IsProcessRunning = true;

            if (IsTimerInclude) _watch = new Util.Stopwatcher(DisplayString);

            DisplayString = DisplayString == "" ? String.Format("Process reading <{0}> objects", ProgressLimit) : displayString;

            base.Start(DisplayString); //PluginManager.Name + ": " + 
        }

        public ProgressMeterBar(string displayString = "", int progressLimit = 100)
        {
            DisplayString = displayString;
            ProgressLimit = progressLimit;
        }

        public override void Stop()
        {
            if (_watch != null) _watch.Dispose();
            _watch = null;
            //Log.IsProcessRunning = false;
            Log.Logger.Information($"{nameof(ProgressMeterBar)} process successed");
            base.Stop();
        }

        protected override void Dispose(bool value)
        {
            Stop();
            base.Dispose(value);
        }
    }
}