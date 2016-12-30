using System.Runtime.Serialization;

namespace Statistics.ViewModel
{
    [DataContract]
    public class GraphValue
    {
        [DataMember(Name = "Key")]
        public object Key { get; set; }

        [DataMember(Name = "Value")]
        public object Value { get; set; }

        public GraphValue(object key, object value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}