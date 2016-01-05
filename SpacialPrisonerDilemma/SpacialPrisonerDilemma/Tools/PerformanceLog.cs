using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacialPrisonerDilemma.Tools
{
    public class PerformanceLog
    {
        public List<DateTime> StepStarts;
        public List<DateTime> StepEnds;
        public TimeSpan AllocationTime
        { get; internal set; }

        public TimeSpan[] StepTimes
        {
            get
            {
                var l = new List<TimeSpan>();
                for(int i=0; i<StepEnds.Count; i++)
                {
                    l.Add(StepEnds[i] - StepStarts[i]);
                }
                return l.ToArray();
            }
        }

        public TimeSpan MaxStepTime
        {
            get
            {
                return TimeSpan.FromMilliseconds(StepTimes.Max(x => x.TotalMilliseconds));
            }
        }

        public TimeSpan MinStepTime
        {
            get
            {
                return TimeSpan.FromMilliseconds(StepTimes.Min(x => x.TotalMilliseconds));
            }
        }

        public TimeSpan Average
        {
            get
            {
                return TimeSpan.FromMilliseconds(StepTimes.Sum(x => x.TotalMilliseconds) / StepTimes.Length);
            }
        }

        public TimeSpan Median
        {
            get
            {
                var l = StepTimes.ToList();
                l.Sort();
                if(l.Count%2==1)
                {
                    return l[l.Count / 2];
                }
                else
                {
                    return TimeSpan.FromMilliseconds((l[l.Count / 2] + l[l.Count / 2 - 1]).TotalMilliseconds / 2);
                }
            }
        }

        public TimeSpan StandardDeviation
        {
            get
            {
                return TimeSpan.FromMilliseconds(StepTimes.Select(x => (x.TotalMilliseconds - Average.TotalMilliseconds)).Average(x => x * x));
            }
        }

        public PerformanceLog()
        {
            StepStarts = new List<DateTime>();
            StepEnds = new List<DateTime>();
        }

        public PerformanceLog(TimeSpan allocTime) : this()
        {
            AllocationTime = allocTime;
        }

        public PerformanceLog(DateTime allocStart, DateTime allocEnd) : this(allocEnd - allocStart)
        { }


        bool started = false;
        public void NewStepStart(DateTime stepStart)
        {
            if (started) throw new ArgumentException();
            StepStarts.Add(stepStart);
            started = true;
        }

        public void NewStepEnd(DateTime stepEnd)
        {
            if (!started) throw new ArgumentException();
            StepEnds.Add(stepEnd);
            started = false;
        }
    }
}
