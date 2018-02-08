namespace SocialPoint.Network
{
    public interface ILocalNetworkServerFactory : INetworkServerFactory
    {
        ILocalNetworkServer Server { get; }
    }

    public class LocalNetworkServerFactory : ILocalNetworkServerFactory
    {
        #region INetworkServerFactory implementation

        public ILocalNetworkServer Server { get; private set; }

        INetworkServer INetworkServerFactory.Create()
        {
            Server = new LocalNetworkServer();
            return Server;
        }

        #endregion
    }
}