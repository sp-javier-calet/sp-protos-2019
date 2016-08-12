using SocialPoint.Base;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public interface INetworkClientDelegate
    {
        void OnConnected();
        void OnDisconnected();
        void OnMessageReceived(NetworkMessageData data);
        void OnError(Error err);
    }

    public interface INetworkClient
    {
        bool Connected{ get; }

        void Connect();
        void Disconnect();
        INetworkMessage CreateMessage(NetworkMessageData data);
        void AddDelegate(INetworkClientDelegate dlg);
        void RemoveDelegate(INetworkClientDelegate dlg);
        void RegisterReceiver(INetworkMessageReceiver receiver);
    }
}