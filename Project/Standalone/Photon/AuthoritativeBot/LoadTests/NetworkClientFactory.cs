using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public class NetworkClientFactory : INetworkClientGameFactory
    {
        public object Create(NetworkClientSceneController ctrl, Dictionary<string, string> config)
        {
            string gameName;
            config.TryGetValue("GameName", out gameName);
            var bot = new NetworkClientBot(NetworkClientBot.MatchTypeEnum.OneVsOne, ctrl, gameName);

            return bot;
        }
    }
}
