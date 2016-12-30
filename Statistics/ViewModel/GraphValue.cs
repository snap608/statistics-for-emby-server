namespace Statistics.ViewModel
{
    public class GraphValue
    {
        public object Key { get; set; }

        public object Value { get; set; }

        public GraphValue(object key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}