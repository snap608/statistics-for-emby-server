using System.Collections.Generic;

namespace Statistics.Models.Chart
{
    public class ChartModel
    {
        public List<ChartValueModel> Week { get; set; }

        public ChartModel()
        {
            Week = new List<ChartValueModel>();
        }

        public ChartModel(IEnumerable<string> keys )
        {
            Week = new List<ChartValueModel>();

            foreach (var key in keys)
            {
                Week.Add(new ChartValueModel(key));
            }
        }
    }
}
