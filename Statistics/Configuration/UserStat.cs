using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Configuration
{
    public class UserStat
    {
        public string UserName { get; set; }
        public List<string> TopMovieGenres { get; set; }
        public List<string> TopShowGenres { get; set; }
        public List<int> TopYears { get; set; }
        public string PlayedMovieViewTime { get; set; }
        public string PlayedShowViewTime { get; set; }
        public string PlayedOverallViewTime { get; set; }
        public string MovieViewTime { get; set; }
        public string ShowViewTime { get; set; }
        public string OverallViewTime { get; set; }
        public List<string> LastMoviesSeen { get; set; }
        public List<string> LastShowsSeen { get; set; }

        public UserStat()
        {
            TopMovieGenres = new List<string>();
            TopShowGenres = new List<string>();
            TopYears = new List<int>();
            LastMoviesSeen = new List<string>();
            LastShowsSeen = new List<string>();
        }
    }
}
