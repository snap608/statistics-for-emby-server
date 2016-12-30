using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Statistics.Extentions
{
    public static class ToJson
    {
        public static string ToJSON(this object obj)
        {
            if (obj == null)
                return string.Empty;
            using (var memoryStream = new MemoryStream())
            {
                new DataContractJsonSerializer(obj.GetType()).WriteObject((Stream)memoryStream, obj);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }
}