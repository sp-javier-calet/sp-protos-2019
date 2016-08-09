using SocialPoint.Base;

namespace SocialPoint.Multiplayer
{
    public interface INetworkClientDelegate
    {
        void OnConnectedToServer();
        void OnDisconnectedFromServer();
        void OnMessageReceived(ReceivedNetworkMessage msg);
        void OnError(Error err);
    }

    public interface INetworkClient
    {
        void Connect();
        void Disconnect();
        INetworkMessage CreateMessage(byte type, int channelId);
        void AddDelegate(INetworkClientDelegate dlg);
        void RemoveDelegate(INetworkClientDelegate dlg);
    }
}
