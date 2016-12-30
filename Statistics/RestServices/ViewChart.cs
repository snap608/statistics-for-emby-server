using MediaBrowser.Model.Services;
using Statistics.Enum;

namespace Statistics.RestServices
{
    [Route("/Statistics/ViewChart/{TimeRange}/{Id}", "GET")]
    public class ViewChart
    {
        [ApiMember(DataType = "string", Description = "user id", IsRequired = true, Name = "Id", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }

        [ApiMember(DataType = "string", Description = "time type", IsRequired = true, Name = "Timerange", ParameterType = "path", Verb = "GET")]
        public TimeRangeEnum TimeRange { get; set; }
    }
}