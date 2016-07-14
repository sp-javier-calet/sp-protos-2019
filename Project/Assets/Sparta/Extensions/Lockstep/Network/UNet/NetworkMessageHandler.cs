using System.Collections;
using System.IO;
using UnityEngine.Networking;
using System;
using SocialPoint.Lockstep.Network;
using SocialPoint.Utils.Network.UNet;

namespace SocialPoint.Lockstep.Network.UNet
{
    public abstract class BaseNetworkMessageHandler
    {
        public short MsgType;

        public BaseNetworkMessageHandler(short msgType)
        {
            MsgType = msgType;
        }

        protected abstract void MessageHandler(NetworkMessage msg);

        public void Register(NetworkClient client)
        {
            client.RegisterHandler(MsgType, MessageHandler);
        }

        public void Unregister(NetworkClient client)
        {
            client.UnregisterHandler(MsgType);
        }

        public void RegisterServer(NetworkServerSimple server)
        {
            server.RegisterHandler(MsgType, MessageHandler);
        }

        public void UnregisterServer(NetworkServerSimple server)
        {
            server.UnregisterHandler(MsgType);
        }
    }

    public class NetworkMessageHandler : BaseNetworkMessageHandler
    {
        Action<NetworkMessageData> _handler;

        public NetworkMessageHandler(short msgType, Action<NetworkMessageData> handler)
            : base(msgType)
        {
            _handler = handler;
        }

        protected override void MessageHandler(NetworkMessage msg)
        {
            _handler(new NetworkMessageData {
                ConnectionId = msg.conn.connectionId,
                Reader = new NetworkReaderWrapper(msg.reader)
            });
        }
    }

    public class SyncNetworkMessageHandler : BaseNetworkMessageHandler
    {
        Action<SyncNetworkMessageData> _handler;

        public SyncNetworkMessageHandler(short msgType, Action<SyncNetworkMessageData> handler)
            : base(msgType)
        {
            _handler = handler;
        }

        protected override void MessageHandler(NetworkMessage msg)
        {
            var reader = new NetworkReaderWrapper(msg.reader);
            int networkTimestamp = reader.ReadInt32();
            byte error;
            int serverDelay = msg.conn.hostId != -1 ? NetworkTransport.GetRemoteDelayTimeMS(msg.conn.hostId, msg.conn.connectionId, networkTimestamp, out error) : 0;
            _handler(new SyncNetworkMessageData {
                ConnectionId = msg.conn.connectionId,
                ServerDelay = serverDelay,
                Reader = reader
            });
        }
    }
}