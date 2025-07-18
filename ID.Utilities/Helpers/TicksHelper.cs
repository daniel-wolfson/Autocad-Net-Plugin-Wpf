using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ID.Utilities.Helpers
{
    //[ProtoContract]
    public class TicksTrail
    {
        private readonly double _trailOffsetFromStartTime;
        //[ProtoMember(1)]
        public DateTime StartTime;
        //[ProtoMember(2)]
        public double LastExecutionTimeInMS;
        //[ProtoMember(4)]
        public List<TrailParam> TrailParams;
        //[ProtoMember(5)]
        public List<Tick> TicksList;

        //[ProtoMember(3)]
        private Stopwatch Stopwatch { get; }

        public long TotalTrailTimeMS => Convert.ToInt64(this.Stopwatch.Elapsed.TotalMilliseconds + this._trailOffsetFromStartTime);

        public TicksTrail(List<TrailParam> trailParams)
          : this(DateTime.UtcNow, trailParams)
        {
        }

        public TicksTrail()
          : this(DateTime.UtcNow, new List<TrailParam>())
        {
        }

        public TicksTrail(string desc)
          : this()
        {
            this.AddTick(desc);
        }

        public TicksTrail(TicksTrail ticks)
        {
            this._trailOffsetFromStartTime = 0.0;
            this.StartTime = ticks.StartTime;
            this.Stopwatch = ticks.Stopwatch;
            this.LastExecutionTimeInMS = ticks.LastExecutionTimeInMS;
            this.TrailParams = ticks.TrailParams.ToList<TrailParam>();
            this.TicksList = ticks.TicksList.ToList<Tick>();
        }

        public TicksTrail(DateTime operationStartTime, List<TrailParam> trailParams)
        {
            this._trailOffsetFromStartTime = (DateTime.UtcNow - operationStartTime).TotalMilliseconds;
            this.StartTime = operationStartTime;
            this.Stopwatch = Stopwatch.StartNew();
            this.LastExecutionTimeInMS = 0.0;
            this.TrailParams = trailParams;
            this.TicksList = new List<Tick>();
        }

        public TicksTrail(DateTime operationStartTime, string desc)
          : this(operationStartTime, new List<TrailParam>())
        {
            this.AddTick(desc, this._trailOffsetFromStartTime);
        }

        public void AddTick(string desc) => this.AddTick(desc, 0.0);

        private void AddTick(string desc, double offset)
        {
            double totalMilliseconds = this.Stopwatch.Elapsed.TotalMilliseconds;
            long int64 = Convert.ToInt64(totalMilliseconds - this.LastExecutionTimeInMS + offset);
            this.TicksList.Add(new Tick(desc, int64));
            this.LastExecutionTimeInMS = totalMilliseconds;
        }

        public static void AddTrailTick(TicksTrail trail, string str) => trail?.AddTick(str, 0.0);

        public static void AddTrailTick(
          TicksTrail trail,
          string currentSample,
          string fromSample,
          string performanceCounterName)
        {
            if (trail == null)
                return;
            trail.AddTick(currentSample, 0.0);
            //TicksTrailPerformanceCounters.SendValueToPerformanceCounter(trail, fromSample, performanceCounterName);
        }

        public static object GetTickSample() => (object)DateTime.UtcNow;

        public static void AddTrailTickSample(TicksTrail trail, string str, object tickSample)
        {
            DateTime dateTime = (DateTime)tickSample;
            DateTime tickSample1 = (DateTime)TicksTrail.GetTickSample();
            if (trail == null || !(dateTime <= tickSample1) || !(dateTime >= trail.StartTime))
                return;
            trail.AddTick(str, dateTime.Subtract(trail.StartTime).TotalMilliseconds);
        }

        public void AddParam<T>(string paramName, T paramValue)
        {
            string pval = (object)paramValue == null ? string.Empty : paramValue.ToString();
            this.TrailParams.Add(new TrailParam(paramName, pval));
        }

        public static void AddTrailParam(TicksTrail trail, string paramName, string paramValue) => trail?.TrailParams.Add(new TrailParam(paramName, paramValue));

        public void Clear() => this.TicksList.Clear();

        public void Stop() => this.Stopwatch.Stop();

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format("Total operation time: {0} ms", (object)this.TotalTrailTimeMS));
            if (this.TrailParams.Count > 0)
                stringBuilder.Append(", ");
            for (int index = 0; index < this.TrailParams.Count; ++index)
                stringBuilder.Append(this.TrailParams[index].ParamName + ": " + this.TrailParams[index].ParamValue + " ");
            if (this.TicksList.Count > 0)
                stringBuilder.Append(", ");
            for (int index = 0; index < this.TicksList.Count; ++index)
            {
                stringBuilder.Append(string.Format("{0} (d: {1} ms)", (object)this.TicksList[index].TickDesc, (object)this.TicksList[index].TickDelta));
                if (index < this.TicksList.Count - 1)
                    stringBuilder.Append(", ");
            }
            return stringBuilder.ToString();
        }

        public string ToStringAsJSON()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{");
            stringBuilder.Append(string.Format("\"StartTime\":\"{0:O}\", ", (object)this.StartTime));
            stringBuilder.Append(string.Format("\"TotalOpTime\":\"{0}\"", (object)this.TotalTrailTimeMS));
            if (this.TrailParams.Count > 0)
                stringBuilder.Append(", \"Params\": {");
            for (int index = 0; index < this.TrailParams.Count; ++index)
            {
                //string format = string.IsNullOrEmpty(this.TrailParams[index].ParamValue) || !this.TrailParams[index].ParamValue.IsValidJson() ? "\"{0}\":\"{1}\"" : "\"{0}\":{1}";
                //stringBuilder.Append(string.Format(format, (object)this.TrailParams[index].ParamName, (object)this.TrailParams[index].ParamValue));
                if (index < this.TrailParams.Count - 1)
                    stringBuilder.Append(", ");
                else
                    stringBuilder.Append("} ");
            }
            if (this.TicksList.Count > 0)
                stringBuilder.Append(", \"Ticks\": {");
            for (int index = 0; index < this.TicksList.Count; ++index)
            {
                stringBuilder.Append(string.Format("\"{0}\":\"{1}\"", (object)this.TicksList[index].TickDesc, (object)this.TicksList[index].TickDelta));
                if (index < this.TicksList.Count - 1)
                    stringBuilder.Append(", ");
                else
                    stringBuilder.Append("} ");
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        public string ToStringAsCSV()
        {
            StringBuilder stringBuilder = new StringBuilder();
            long num = 0;
            foreach (Tick ticks in this.TicksList)
            {
                if (num != 0L)
                    stringBuilder.Append(string.Format("{0}, ", (object)ticks.TickDelta));
                num = 1L;
            }
            return stringBuilder.ToString();
        }
    }

    //[ProtoContract]
    public struct TrailParam
    {
        //[ProtoMember(1)]
        public string ParamName;
        //[ProtoMember(2)]
        public string ParamValue;

        public TrailParam(string pname, string pval)
        {
            this.ParamName = pname;
            this.ParamValue = pval;
        }

        public override string ToString() => "ParamName: " + this.ParamName + ", ParamValue: " + this.ParamValue;
    }

    //[ProtoContract]
    public struct Tick
    {
        //[ProtoMember(1)]
        public string TickDesc;
        //[ProtoMember(2)]
        public long TickDelta;

        public Tick(string desc, long prevTickDelta)
        {
            this.TickDesc = desc;
            this.TickDelta = prevTickDelta;
        }

        public override string ToString() => string.Format("{0}: {1}, {2}: {3}", (object)"TickDesc", (object)this.TickDesc, (object)"TickDelta", (object)this.TickDelta);
    }

    public class TicksTrailPerformanceCounters
    {
        public static string PCS_CATEGORY_RATE_TIME_PROCESS_PREFIX = "ProcessRateTime_Account_{0}";
        private static ConcurrentDictionary<string, Tuple<PerformanceCounter, PerformanceCounter>> _performanceCounters = new ConcurrentDictionary<string, Tuple<PerformanceCounter, PerformanceCounter>>();

        public static void AddPerformanceCounters(
          string performanceCounterName,
          PerformanceCounter averageCounter,
          PerformanceCounter baseAverageCounter)
        {
            if (TicksTrailPerformanceCounters._performanceCounters.TryGetValue(performanceCounterName, out Tuple<PerformanceCounter, PerformanceCounter> _))
                return;
            Tuple<PerformanceCounter, PerformanceCounter> tuple = new Tuple<PerformanceCounter, PerformanceCounter>(averageCounter, baseAverageCounter);
            TicksTrailPerformanceCounters._performanceCounters.TryAdd(performanceCounterName, tuple);
        }

        internal static void SendValueToPerformanceCounter(
          TicksTrail ticksTrail,
          string startSamplePoint,
          string performanceCounterName)
        {
            Tuple<PerformanceCounter, PerformanceCounter> tuple;
            if (!TicksTrailPerformanceCounters._performanceCounters.TryGetValue(performanceCounterName, out tuple))
                return;
            PerformanceCounter performanceCounter1 = tuple.Item1;
            PerformanceCounter performanceCounter2 = tuple.Item2;
            long num = ticksTrail.TicksList.SkipWhile<Tick>((Func<Tick, bool>)(item => item.TickDesc != startSamplePoint)).Skip<Tick>(1).Sum<Tick>((Func<Tick, long>)(item => item.TickDelta));
            performanceCounter1.IncrementBy(num * performanceCounter1.NextSample().CounterFrequency);
            performanceCounter2.Increment();
        }
    }

}
