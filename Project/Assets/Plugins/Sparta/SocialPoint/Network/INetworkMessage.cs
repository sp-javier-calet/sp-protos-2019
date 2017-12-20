using SocialPoint.IO;
using SocialPoint.Utils;
using System.Collections.Generic;
using System.Text;

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
        public List<byte> ClientIds;
        public int MessageLength;

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
            if(ClientIds != null)
            {
                for(int i = 0; i < ClientIds.Count; ++i)
                {
                    hash = CryptographyUtils.HashCombine(hash, ClientIds[i].GetHashCode());
                }
            }
            return hash;
        }

        public static bool operator ==(NetworkMessageData a, NetworkMessageData b)
        {
            bool isEqual = a.MessageType == b.MessageType && a.Unreliable == b.Unreliable;
            if(a.ClientIds != null && b.ClientIds != null)
            {
                isEqual &= a.ClientIds.Count == b.ClientIds.Count;
                if(isEqual)
                {
                    for(int i = 0; i < a.ClientIds.Count; ++i)
                    {
                        isEqual &= a.ClientIds[i] == b.ClientIds[i];
                    }
                }
            }
            return isEqual;
        }

        public static bool operator !=(NetworkMessageData a, NetworkMessageData b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            var sb = new StringBuilder(128);
            sb.Append(string.Format("[NetworkMessageData MessageType={0} Unreliable={1}",MessageType,Unreliable));
            if(ClientIds != null)
            {
                for(int i = 0; i < ClientIds.Count; ++i)
                {
                    sb.Append(string.Format(" ClientId{0}={1}", i, ClientIds[i]));
                }
            }
            sb.Append("]");

            return sb.ToString();
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

        public static void SendMessage(this INetworkMessageSender sender, INetworkShareable obj)
        {
            sender.SendMessage(new NetworkMessageData{ }, obj);
        }

        public static void SendMessage(this INetworkMessageSender sender, NetworkMessageData data, byte[] body)
        {
            var msg = sender.CreateMessage(data);
            msg.Writer.Write(body, body.Length);
            msg.Send();
        }


        public static void SendMessage(this INetworkMessageSender sender, byte[] body)
        {
            sender.SendMessage(new NetworkMessageData{ }, body);
        }
    }
}