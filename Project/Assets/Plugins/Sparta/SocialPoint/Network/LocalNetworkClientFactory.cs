namespace SocialPoint.Network
{
    public class LocalNetworkClientFactory : BaseNetworkClientFactory<LocalNetworkClient>
    {
        readonly LocalNetworkServerFactory _serverFactory;

        public LocalNetworkClientFactory(LocalNetworkServerFactory serverFactory)
        {
            _serverFactory = serverFactory;
        }

        protected override LocalNetworkClient DoCreate()
        {
            return new LocalNetworkClient(_serverFactory.Create());
        }
    }
}