using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class UnetNetworkClientFactory : BaseNetworkClientFactory<UnetNetworkClient>
    {
        readonly UnetNetworkInstaller.SettingsData _settings;

        public UnetNetworkClientFactory(
            UnetNetworkInstaller.SettingsData settings,
            List<INetworkClientDelegate> delegates) : base(delegates)
        {
            _settings = settings;
        }

        protected override UnetNetworkClient DoCreate()
        {
            return new UnetNetworkClient(_settings.Config.ServerAddress, _settings.Config.ServerPort);
        }
    }
}