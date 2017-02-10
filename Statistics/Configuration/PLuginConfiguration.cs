using System;
using System.Collections.Generic;
using MediaBrowser.Model.Plugins;
using Statistics.ViewModel;

namespace Statistics.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public PluginConfiguration()
        {
            UserStats = new List<UserStat>();
            GeneralStat = new List<ValueGroup>();
        }
        public List<UserStat> UserStats { get; set; }
        public List<ValueGroup> GeneralStat { get; set; }
        public string LastUpdated { get; set; }
    }
}
