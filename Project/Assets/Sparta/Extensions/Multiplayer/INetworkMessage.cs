
using SocialPoint.IO;
using System.IO;

namespace SocialPoint.Multiplayer
{
    public interface INetworkMessage
    {
        IWriter Writer{ get; }
        void Send();
    }

    public struct NetworkMessageData
    {
        public byte MessageType;
        public int ChannelId;
        public int ClientId;

        public override bool Equals(System.Object obj)
        {
            return this == (NetworkMessageData)obj;
        }

        public bool Equals(NetworkMessageData data)
        {             
            return this == data;
        }

        public override int GetHashCode()
        {
            return MessageType.GetHashCode() ^ ChannelId.GetHashCode() ^ ClientId.GetHashCode();
        }

        public static bool operator ==(NetworkMessageData a, NetworkMessageData b)
        {
            return a.MessageType == b.MessageType && a.ChannelId == b.ChannelId && a.ClientId == b.ClientId;
        }

        public static bool operator !=(NetworkMessageData a, NetworkMessageData b)
        {
            return !(a == b);
        }            

        public override string ToString()
        {
            return string.Format("[NetworkMessageData MessageType={0} ChannelId={1} ClientId={2}]",
                MessageType, ChannelId, ClientId);
        }
    }

    public interface INetworkMessageReceiver
    {
        void OnMessageReceived(NetworkMessageData data, IReader reader);
    }
}