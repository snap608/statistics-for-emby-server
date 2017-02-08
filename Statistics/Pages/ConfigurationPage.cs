using System;
using System.IO;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Plugins;

namespace Statistics.Configuration
{
    public class ConfigurationPage : IPluginConfigurationPage
    {
        public string Name => Constants.Name;

        public ConfigurationPageType ConfigurationPageType => ConfigurationPageType.PluginConfiguration;

        public IPlugin Plugin => Statistics.Plugin.Instance;

        public Stream GetHtmlStream()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".configPage.html");
        }
    }
}
