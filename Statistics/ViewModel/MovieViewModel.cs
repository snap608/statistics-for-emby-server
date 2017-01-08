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
            return string.IsNullOrEmpty(UserName) ? 
                $"{Name} - {Played.ToString("d", Thread.CurrentThread.CurrentCulture)}" : 
                $"{Name} - {Played.ToString("d", Thread.CurrentThread.CurrentCulture)} - Viewed by {UserName}";
        }
    }
}