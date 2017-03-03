namespace Statistics.Models.Chart
{
    public class ChartValueModel
    {
        public int Movies { get; set; }
        public int Episodes { get; set; }
        public string Key { get; set; }

        public ChartValueModel()
        {
            
        }

        public ChartValueModel(int movies, int episodes, string key)
        {
            Movies = movies;
            Episodes = episodes;
            Key = key;
        }

        public ChartValueModel(string key)
        {
            Key = key;
        }
    }
}
