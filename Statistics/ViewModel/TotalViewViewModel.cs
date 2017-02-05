using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.ViewModel
{
    public class TotalViewViewModel
    {
        public string UserName { get; set; }
        public RunTime TimeSpan { get; set; }

        public TotalViewViewModel(string userName, RunTime timeSpan)
        {
            UserName = userName;
            TimeSpan = timeSpan;
        }

        public override string ToString()
        {
            return $"{UserName} - {TimeSpan}";
        }
    }
}
