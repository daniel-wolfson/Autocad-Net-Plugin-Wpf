using System;
using System.Diagnostics;

namespace ID.Infrastructure.Helpers
{
    public partial class Util
    {
        /// <summary> Stopwatcher </summary>
        public class Stopwatcher : IDisposable
        {
            private Stopwatch _watch;
            private readonly string _displayString = "";

            /// <summary> Constructor </summary>
            /// <param name="displayString"></param>
            public Stopwatcher(string displayString = "")
            {
                //Log.Logger.Information("", "Watcher started");
                _displayString = displayString != "" ? displayString + " " : displayString;
                _watch = new Stopwatch();
                _watch.Start();
            }

            public void Dispose()
            {
                _watch.Stop();

                //Correction time estimate on depend of time single operaton
                var ts = TimeSpan.FromSeconds(_watch.Elapsed.TotalSeconds);
                if (ts.TotalSeconds - TimeEstimater.TimeEstimate.TotalSeconds > 10)
                {
                    TimeEstimater.TimeMachineFactorForSingleOperation = TimeEstimater.TimeMachineFactorForSingleOperation - TimeEstimater.TimeEstimate.TotalSeconds / ts.TotalSeconds / 100;
                }
                //Log.Logger.Information("", "Watcher: Time elapsed: {0}",
                //    new TimeSpan(ts.Hours, ts.Minutes,
                //        Math.Abs(_watch.Elapsed.TotalSeconds - 1) < 1 ? 1 : ts.Seconds));
                _watch = null;
            }
        }

        public class TimeEstimater : IDisposable
        {
            public static double TimeMachineFactorForSingleOperation = 0.3;
            public static TimeSpan TimeEstimate;
            public static bool IsTimeEstimateMode;

            public TimeEstimater(string description = "")
            {
                IsTimeEstimateMode = true;
            }

            public static TimeSpan CountToEstimateTimeSpan(int objectCount)
            {
                var ts = TimeSpan.FromMilliseconds(objectCount * TimeMachineFactorForSingleOperation * 5);
                return new TimeSpan(0, ts.Minutes, ts.Seconds);
            }

            public void Dispose()
            {
                IsTimeEstimateMode = false;
            }
        }

    }
}