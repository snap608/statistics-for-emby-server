using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Plugins;
using System;
using System.IO;

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
                return (IPlugin)Statistics.Plugin.Instance;
            }
        }

        public Stream GetHtmlStream()
        {
            Type type = this.GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".configPage.html");
        }
    }
}
