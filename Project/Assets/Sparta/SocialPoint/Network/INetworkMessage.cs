﻿
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
        public byte ChannelId;
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
            hash = CryptographyUtils.HashCombine(hash, ChannelId.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, ClientId.GetHashCode());
            return hash;
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