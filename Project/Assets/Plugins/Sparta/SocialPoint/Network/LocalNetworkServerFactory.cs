namespace SocialPoint.Network
{
    public class LocalNetworkServerFactory : BaseNetworkServerFactory<LocalNetworkServer>
    {
        LocalNetworkServer _server;

        protected override LocalNetworkServer DoCreate()
        {
            if(_server == null)
            {
                _server = new LocalNetworkServer();
            }
            return _server;
        }
    }
}