using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;

namespace Statistics.Configuration
{
    public class ShowProgress
    {
        public string Name { get; set; }
        public string StartYear { get; set; }
        public string Progress { get; set; }
        public float? Score { get; set; }
        public SeriesStatus? Status { get; set; }
    }
}
