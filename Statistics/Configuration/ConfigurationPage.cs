using System;
using System.IO;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Plugins;

namespace Statistics.Configuration
{
    public class ConfigurationPage : IPluginConfigurationPage
    {
        public string Name
        {
            get
            {
                return "Statistics".Replace(" ", "");
            }
        }

        public ConfigurationPageType ConfigurationPageType
        {
            get
            {
                return ConfigurationPageType.PluginConfiguration;
            }
        }

        public IPlugin Plugin
        {
            get
            {
                return Statistics.Plugin.Instance;
            }
        }

        public Stream GetHtmlStream()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".configPage.html");
        }
    }
}
