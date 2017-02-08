using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Plugins;

namespace Statistics.Configuration
{
    public class UserBasedStatistics : IPluginConfigurationPage
    {
        public string Name => Constants.MainStatisticsName;

        public ConfigurationPageType ConfigurationPageType => ConfigurationPageType.PluginConfiguration;

        public IPlugin Plugin => Statistics.Plugin.Instance;

        public Stream GetHtmlStream()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".UserBasedStatisticsPage.html");
        }
    }
}
