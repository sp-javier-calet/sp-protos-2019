using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class UnetNetworkClientFactory : BaseNetworkClientFactory, INetworkClientFactory
    {
        readonly UnetNetworkInstaller.SettingsData _settings;

        public UnetNetworkClientFactory(
            UnetNetworkInstaller.SettingsData settings,
            List<INetworkClientDelegate> delegates) : base(delegates)
        {
            _settings = settings;
        }

        #region INetworkClientFactory implementation

        INetworkClient INetworkClientFactory.Create()
        {
            return Create();
        }

        #endregion

        public UnetNetworkClient Create()
        {
            return Create<UnetNetworkClient>(new UnetNetworkClient(_settings.Config.ServerAddress, _settings.Config.ServerPort));
        }
    }
}