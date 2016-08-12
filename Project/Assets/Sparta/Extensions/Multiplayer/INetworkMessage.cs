﻿
using SocialPoint.IO;
using System.IO;

namespace SocialPoint.Multiplayer
{
    public interface INetworkMessage
    {
        IWriter Writer{ get; }
        void Send();
    }

    public struct NetworkMessageDest
    {
        public byte MessageType;
        public int ChannelId;
        public int ClientId;
    }
}