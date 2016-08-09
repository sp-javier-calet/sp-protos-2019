using SocialPoint.Base;

namespace SocialPoint.Multiplayer
{
    public interface INetworkServerDelegate
    {
        void OnStarted();
        void OnClientConnected(byte clientId);
        void OnClientDisconnected(byte clientId);
        void OnMessageReceived(byte clientId, ReceivedNetworkMessage msg);
        void OnError(Error err);
    }

    public interface INetworkServer
    {
        void Start();
        void Stop();
        INetworkMessage CreateMessage(byte type, int channelId);
        INetworkMessage CreateMessage(byte clientId, byte type, int channelId);
        void AddDelegate(INetworkServerDelegate dlg);
        void RemoveDelegate(INetworkServerDelegate dlg);
    }
}
