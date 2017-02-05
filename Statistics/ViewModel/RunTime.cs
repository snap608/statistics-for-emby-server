using System;
using Statistics.Enum;

namespace Statistics.ViewModel
{
    public class RunTime
    {
        private TimeSpan _timeSpan;
        public int Days => _timeSpan.Days;
        public int Hours => _timeSpan.Hours;
        public int Minutes => _timeSpan.Minutes;
        public int Seconds => _timeSpan.Seconds;
        public long Ticks => _timeSpan.Ticks;

        public RunTime(TimeSpan timeSpan = new TimeSpan())
        {
            _timeSpan = timeSpan;
        }

        public void Add(TimeSpan timespan)
        {
            _timeSpan = _timeSpan.Add(timespan);
        }

        public void Add(long? ticks)
        {
            _timeSpan = _timeSpan.Add(new TimeSpan(ticks ?? 0));
        }

        public override string ToString()
        {
            var days = Days != 1
                ? $"{Days} days"
                : $"{Days} day";
            var hours = Hours != 1 
                ? $"{Hours} hours"
                : $"{Hours} hour";
            var minutes = Minutes != 1
                ? $"{Minutes} minutes"
                : $"{Minutes} minute";
            return $"{days}, {hours} and {minutes}";
        }
    }
}
