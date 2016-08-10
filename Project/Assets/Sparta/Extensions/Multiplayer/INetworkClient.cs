using SocialPoint.Base;

namespace SocialPoint.Multiplayer
{
    public interface INetworkClientDelegate
    {
        void OnConnected();
        void OnDisconnected();
        void OnMessageReceived(ReceivedNetworkMessage msg);
        void OnError(Error err);
    }

    public interface INetworkClient
    {
        void Connect();
        void Disconnect();
        INetworkMessage CreateMessage(NetworkMessageInfo data);
        void AddDelegate(INetworkClientDelegate dlg);
        void RemoveDelegate(INetworkClientDelegate dlg);
    }
}
