using System.Collections.Generic;
using Photon.Hive.Plugin;

public class CaPluginFactory : IPluginFactory
{
    public IGamePlugin Create(IPluginHost gameHost, string pluginName, Dictionary<string, string> config, out string errorMsg)
    {
        var plugin = new CaAuthoritativePlugin();
        if(plugin.SetupInstance(gameHost, config, out errorMsg))
        {
            return plugin;
        }
        return null;
    }
}
