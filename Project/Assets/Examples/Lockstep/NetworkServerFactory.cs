using System;
using System.Collections.Generic;
using SocialPoint.Lockstep;

namespace Examples.Lockstep
{
    public class NetworkServerFactory : INetworkServerGameFactory
    {
        long GetConfig(Dictionary<string, string> config, string k, long def)
        {
            string str;
            if(config.TryGetValue(k, out str))
            {
                long l;
                if(long.TryParse(str, out l))
                {
                    return l;
                }
            }
            return def;
        }

        public object Create(LockstepNetworkServer server, Dictionary<string, string> config)
        {
            var gameConfig = new Config();
            gameConfig.Duration = GetConfig(config, "Duration", gameConfig.Duration);
            gameConfig.ManaSpeed = GetConfig(config, "ManaSpeed", gameConfig.ManaSpeed);
            gameConfig.MaxMana = GetConfig(config, "MaxMana", gameConfig.MaxMana);
            gameConfig.UnitCost = GetConfig(config, "UnitCost", gameConfig.UnitCost);
            return new ServerBehaviour(server, gameConfig);
        }
    }
}