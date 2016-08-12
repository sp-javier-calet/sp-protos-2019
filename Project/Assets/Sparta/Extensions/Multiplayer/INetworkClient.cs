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
        bool Connected{ get; }
        void Connect();
        void Disconnect();
        INetworkMessage CreateMessage(NetworkMessageDest data);
        void AddDelegate(INetworkClientDelegate dlg);
        void RemoveDelegate(INetworkClientDelegate dlg);
    }
}