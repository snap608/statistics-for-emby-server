namespace Statistics.Configuration
{
    public class ValueGroup
    {
        public string Title { get; set; }

        public string Value { get; set; }
        public string Size { get; set; }
        public object Raw { get; set; }

        public ValueGroup()
        {
            Size = "small";
        }
    }
}