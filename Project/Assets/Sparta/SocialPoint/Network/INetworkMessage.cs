
using SocialPoint.IO;
using SocialPoint.Utils;
using System.IO;

namespace SocialPoint.Network
{
    public interface INetworkMessage
    {
        IWriter Writer{ get; }

        void Send();
    }

    public struct NetworkMessageData
    {
        public byte MessageType;
        public bool Unreliable;
        public byte ClientId;

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
            int hash = MessageType.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, Unreliable.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, ClientId.GetHashCode());
            return hash;
        }

        public static bool operator ==(NetworkMessageData a, NetworkMessageData b)
        {
            return a.MessageType == b.MessageType && a.Unreliable == b.Unreliable && a.ClientId == b.ClientId;
        }

        public static bool operator !=(NetworkMessageData a, NetworkMessageData b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return string.Format("[NetworkMessageData MessageType={0} Unreliable={1} ClientId={2}]",
                MessageType, Unreliable, ClientId);
        }
    }

    public interface INetworkMessageReceiver
    {
        void OnMessageReceived(NetworkMessageData data, IReader reader);
    }

    public interface INetworkMessageSender
    {
        INetworkMessage CreateMessage(NetworkMessageData data);
    }


    public static class NetworkMessageSenderExtensions
    {
        public static void SendMessage(this INetworkMessageSender sender, NetworkMessageData data, INetworkShareable obj)
        {
            var msg = sender.CreateMessage(data);
            obj.Serialize(msg.Writer);
            msg.Send();
        }

        public static void SendMessage(this INetworkMessageSender sender, NetworkMessageData data, byte[] body)
        {
            var msg = sender.CreateMessage(data);
            msg.Writer.Write(body, body.Length);
            msg.Send();
        }
    }
}