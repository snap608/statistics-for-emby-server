using Statistics.Enum;
using System;

namespace Statistics.ViewModel
{
    public class RunTime
    {
        private TimeSpan _timeSpan;
        private VideoStateEnum _watched;

        public int Days
        {
            get
            {
                return this._timeSpan.Days;
            }
        }

        public int Hours
        {
            get
            {
                return this._timeSpan.Hours;
            }
        }

        public int Minutes
        {
            get
            {
                return this._timeSpan.Minutes;
            }
        }

        public int Seconds
        {
            get
            {
                return this._timeSpan.Seconds;
            }
        }

        public long Ticks
        {
            get
            {
                return this._timeSpan.Ticks;
            }
        }

        public RunTime(TimeSpan timeSpan, VideoStateEnum watched)
        {
            this._timeSpan = timeSpan;
            this._watched = watched;
        }

        public void Add(TimeSpan timespan)
        {
            this._timeSpan = this._timeSpan.Add(timespan);
        }

        public override string ToString()
        {
            string str1 = string.Format("{0} days, ", (object)this.Days);
            string str2 = this.Hours.ToString().Length != 1 ? str1 + string.Format("{0} hours, ", (object)this.Hours) : str1 + string.Format("{0} hour, ", (object)this.Hours);
            return this.Minutes.ToString().Length != 1 ? str2 + string.Format("and {0} minutes ", (object)this.Minutes) : str2 + string.Format("and {0} minute ", (object)this.Minutes);
        }
    }
}
