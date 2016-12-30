using MediaBrowser.Model.Services;

namespace Statistics.RestServices
{
    [Route("/Statistics/GetBackground/{Count}", "GET")]
    public class GetBackground
    {
        [ApiMember(DataType = "string", Description = "Item Count", IsRequired = true, Name = "Count", ParameterType = "path", Verb = "GET")]
        public string Count { get; set; }
    }
}