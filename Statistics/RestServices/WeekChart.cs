using MediaBrowser.Model.Services;

namespace Statistics.RestServices
{
    [Route("/Statistics/WeekChart/{Id}", "GET")]
    public class WeekChart
    {
        [ApiMember(DataType = "string", Description = "user id", IsRequired = true, Name = "Id", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }
    }
}