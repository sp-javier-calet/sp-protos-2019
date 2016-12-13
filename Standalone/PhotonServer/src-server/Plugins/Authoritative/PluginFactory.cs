using System.Collections.Generic;
using Photon.Hive.Plugin;

namespace SocialPoint.Multiplayer
{
    public class PluginFactory : IPluginFactory
    {
        public IGamePlugin Create(IPluginHost gameHost, string pluginName, Dictionary<string, string> config, out string errorMsg)
        {
            var plugin = new AuthoritativePlugin();
            if(plugin.SetupInstance(gameHost, config, out errorMsg))
            {
                return plugin;
            }
            return null;
        }
    }
}
