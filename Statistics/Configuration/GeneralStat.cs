using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statistics.Enum;
using Statistics.Models;

namespace Statistics.Configuration
{
    public class GeneralStat
    {
        public int TotalMovies { get; set; }
        public int TotalShows { get; set; }
        public int TotalEpisodes { get; set; }
        public List<VideoQualityModel> MovieQualities { get; set; }
        public List<VideoQualityModel> EpisodeQualities { get; set; }
    }
}
