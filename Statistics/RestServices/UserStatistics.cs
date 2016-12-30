using MediaBrowser.Model.Services;

namespace Statistics.RestServices
{
    [Route("/Statistics/UserStatistics/{Id}", "GET")]
    public class UserStatistics
    {
        [ApiMember(DataType = "string", Description = "Item Id", IsRequired = true, Name = "Id", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }
    }
}