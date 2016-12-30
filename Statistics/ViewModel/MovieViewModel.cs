using System;
using System.Threading;

namespace Statistics.ViewModel
{
    public class MovieViewModel
    {
        public string Name { get; set; }

        public DateTime Played { get; set; }

        public string UserName { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(UserName))
                return string.Format("{0} - {1}", Name, Played.ToString("d", Thread.CurrentThread.CurrentCulture));
            return string.Format("{0} - {1} - Viewed by {2}", Name, Played.ToString("d", Thread.CurrentThread.CurrentCulture), UserName);
        }
    }
}