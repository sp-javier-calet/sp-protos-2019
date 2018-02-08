namespace SocialPoint.Network
{
    public class LocalNetworkClientFactory : INetworkClientFactory
    {
        readonly ILocalNetworkServerFactory _serverFactory;

        public LocalNetworkClientFactory(ILocalNetworkServerFactory serverFactory)
        {
            _serverFactory = serverFactory;
        }

        #region INetworkClientFactory implementation

        INetworkClient INetworkClientFactory.Create()
        {
            return new LocalNetworkClient(_serverFactory.Server as ILocalNetworkServer);
        }

        #endregion
    }
}