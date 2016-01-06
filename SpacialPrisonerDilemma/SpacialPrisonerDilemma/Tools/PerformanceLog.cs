using System;
using System.Collections.Generic;
using System.Linq;

namespace SpacialPrisonerDilemma.Tools
{
    /// <summary>
    /// Raport wydajnościowy
    /// </summary>
    public class PerformanceLog
    {

        private List<DateTime> StepStarts;
        private List<DateTime> StepEnds;

        /// <summary>
        /// Czas jaki zajęła alokacja SPD
        /// </summary>
        public TimeSpan AllocationTime
        { get; internal set; }

        /// <summary>
        /// Tablica czasów obliczeń kolejnych kroków automatu
        /// </summary>
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

        /// <summary>
        /// Największy czas obliczeń
        /// </summary>
        public TimeSpan MaxStepTime
        {
            get
            {
                return TimeSpan.FromMilliseconds(StepTimes.Max(x => x.TotalMilliseconds));
            }
        }

        /// <summary>
        /// Najmniejszy czas obliczeń
        /// </summary>
        public TimeSpan MinStepTime
        {
            get
            {
                return TimeSpan.FromMilliseconds(StepTimes.Min(x => x.TotalMilliseconds));
            }
        }

        /// <summary>
        /// Średnia czasu obliczeń
        /// </summary>
        public TimeSpan Average
        {
            get
            {
                return TimeSpan.FromMilliseconds(StepTimes.Sum(x => x.TotalMilliseconds) / StepTimes.Length);
            }
        }

        /// <summary>
        /// Mediana czasu obliczeń
        /// </summary>
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

        /// <summary>
        /// Odchylenie standardowe czasu obliczeń
        /// </summary>
        public TimeSpan StandardDeviation
        {
            get
            {
                return TimeSpan.FromMilliseconds(StepTimes.Select(x => (x.TotalMilliseconds - Average.TotalMilliseconds)).Average(x => x * x));
            }
        }

        /// <summary>
        /// Konstruktor
        /// </summary>
        public PerformanceLog()
        {
            StepStarts = new List<DateTime>();
            StepEnds = new List<DateTime>();
        }

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="allocTime">Czas alokacji SPD</param>
        public PerformanceLog(TimeSpan allocTime) : this()
        {
            AllocationTime = allocTime;
        }

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="allocStart">Początek alokacji SPD</param>
        /// <param name="allocEnd">Koniec alokacji SPD</param>
        public PerformanceLog(DateTime allocStart, DateTime allocEnd) : this(allocEnd - allocStart)
        { }


        bool started = false;
        /// <summary>
        /// Dodaj początek nowego kroku
        /// </summary>
        /// <param name="stepStart">Czas rozpoczęcia realizacji nowego kroku</param>
        public void NewStepStart(DateTime stepStart)
        {
            if (started) throw new ArgumentException();
            StepStarts.Add(stepStart);
            started = true;
        }

        /// <summary>
        /// Dodaj koniec aktualnego kroku
        /// </summary>
        /// <param name="stepEnd">Czas zakończenia realizacji kroku</param>
        public void NewStepEnd(DateTime stepEnd)
        {
            if (!started) throw new ArgumentException();
            StepEnds.Add(stepEnd);
            started = false;
        }
    }
}
