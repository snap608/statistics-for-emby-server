using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Statistics.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public PluginConfiguration()
        {
            UserStats = new List<UserStat>();
        }
        public List<UserStat> UserStats { get; set; }
        public GeneralStat GeneralStat { get; set; }
    }
}
