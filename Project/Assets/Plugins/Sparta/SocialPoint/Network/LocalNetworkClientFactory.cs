namespace SocialPoint.Network
{
    public class LocalNetworkClientFactory : BaseNetworkClientFactory, INetworkClientFactory
    {
        readonly ILocalNetworkServerFactory _serverFactory;

        public LocalNetworkClientFactory(ILocalNetworkServerFactory serverFactory)
        {
            _serverFactory = serverFactory;
        }

        #region INetworkClientFactory implementation

        INetworkClient INetworkClientFactory.Create()
        {
            return Create();
        }

        #endregion

        public LocalNetworkClient Create()
        {
            if(_serverFactory.Server == null)
            {
                _serverFactory.Create();
            }
            return Create<LocalNetworkClient>(new LocalNetworkClient(_serverFactory.Server));
        }
    }
}