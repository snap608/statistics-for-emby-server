using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statistics.ViewModel;

namespace Statistics.Configuration
{
    public class UserStat
    {
        public string UserName { get; set; }
        public List<ValueGroup> ValueGroups { get; set; }
        public List<ShowProgress> ShowProgresses { get; set; }

        public UserStat()
        {
            ValueGroups = new List<ValueGroup>();
            ShowProgresses = new List<ShowProgress>();
        }

       
    }
}
