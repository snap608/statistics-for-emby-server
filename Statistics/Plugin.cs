using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Serialization;
using Statistics.Configuration;

namespace Statistics
{
       internal class Plugin : BasePlugin<PluginConfiguration>
    {
     public static Plugin Instance { get; private set; }

        public override string Name => "Statistics";

        public override string Description => "Get statistics from your collection";

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }
    }
}
