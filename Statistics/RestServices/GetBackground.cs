using MediaBrowser.Model.Services;

namespace Statistics.RestServices
{
    [Route("/Statistics/GetBackground/{Count}", "GET")]
    public class GetBackground
    {
        [ApiMember(DataType = "int", Description = "Count", IsRequired = true, Name = "Count", ParameterType = "path", Verb = "GET")]
        public int Count { get; set; }
    }
}