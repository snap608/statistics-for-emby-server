using System;
using Statistics.Enum;

namespace Statistics.ViewModel
{
    public class RunTime
    {
        private TimeSpan _timeSpan;
        public VideoStateEnum Watched { get; }
        public int Days => _timeSpan.Days;
        public int Hours => _timeSpan.Hours;
        public int Minutes => _timeSpan.Minutes;
        public int Seconds => _timeSpan.Seconds;
        public long Ticks => _timeSpan.Ticks;

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
            var days = $"{Days} days, ";
            var hours = Hours != 1 ?
                days + $"{Hours} hours, "
                : days + $"{Hours} hour, ";
            return Minutes != 1 ? 
                hours + $"and {Minutes} minutes "
                : hours + $"and {Minutes} minute ";
        }
    }
}
