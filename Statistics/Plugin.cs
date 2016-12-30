using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using Statistics.Configuration;

namespace Statistics
{
    internal class Plugin : BasePlugin<PluginConfiguration>
    {
        public static Plugin Instance { get; private set; }

        public override string Name
        {
            get
            {
                return "Statistics";
            }
        }

        public override string Description
        {
            get
            {
                return "Get funny statistics from your collection";
            }
        }

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Plugin.Instance = this;
        }
    }
}
