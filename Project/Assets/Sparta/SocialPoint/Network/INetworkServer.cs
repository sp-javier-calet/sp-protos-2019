using SocialPoint.Base;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    /**
     * a real clientId should never be 0
     */
    public interface INetworkServerDelegate
    {
        void OnServerStarted();
        void OnServerStopped();
        void OnClientConnected(byte clientId);
        void OnClientDisconnected(byte clientId);
        void OnMessageReceived(NetworkMessageData data);
        void OnNetworkError(Error err);
    }

    public interface INetworkServer
    {
        bool Running{ get; }
        void Start();
        void Stop();
        INetworkMessage CreateMessage(NetworkMessageData info);
        void AddDelegate(INetworkServerDelegate dlg);
        void RemoveDelegate(INetworkServerDelegate dlg);
        void RegisterReceiver(INetworkMessageReceiver receiver);
    }

    public static class NetworkServerExtensions
    {
        public static void SendMessage(this INetworkServer server, NetworkMessageData data, INetworkShareable obj)
        {
            var msg = server.CreateMessage(data);
            obj.Serialize(msg.Writer);
            msg.Send();
        }
    }
}
