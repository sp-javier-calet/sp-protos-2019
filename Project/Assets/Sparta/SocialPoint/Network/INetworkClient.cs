using SocialPoint.Base;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    public interface INetworkClientDelegate
    {
        void OnClientConnected();

        void OnClientDisconnected();

        void OnMessageReceived(NetworkMessageData data);

        void OnNetworkError(Error err);
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

    public static class NetworkClientExtensions
    {
        public static void SendMessage(this INetworkClient client, NetworkMessageData data, INetworkShareable obj)
        {
            var msg = client.CreateMessage(data);
            obj.Serialize(msg.Writer);
            msg.Send();
        }
    }
}