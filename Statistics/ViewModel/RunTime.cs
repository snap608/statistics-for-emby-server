using System;
using Statistics.Enum;

namespace Statistics.ViewModel
{
    public class RunTime
    {
        private TimeSpan _timeSpan;
        public VideoStateEnum Watched { get; }

        public int Days
        {
            get
            {
                return _timeSpan.Days;
            }
        }

        public int Hours
        {
            get
            {
                return _timeSpan.Hours;
            }
        }

        public int Minutes
        {
            get
            {
                return _timeSpan.Minutes;
            }
        }

        public int Seconds
        {
            get
            {
                return _timeSpan.Seconds;
            }
        }

        public long Ticks
        {
            get
            {
                return _timeSpan.Ticks;
            }
        }

        public RunTime(TimeSpan timeSpan, VideoStateEnum watched)
        {
            _timeSpan = timeSpan;
            Watched = watched;
        }

        public void Add(TimeSpan timespan)
        {
            _timeSpan = _timeSpan.Add(timespan);
        }

        public override string ToString()
        {
            var str1 = string.Format("{0} days, ", Days);
            var str2 = Hours.ToString().Length != 1 ? str1 + string.Format("{0} hours, ", Hours) : str1 + string.Format("{0} hour, ", Hours);
            return Minutes.ToString().Length != 1 ? str2 + string.Format("and {0} minutes ", Minutes) : str2 + string.Format("and {0} minute ", Minutes);
        }
    }
}
