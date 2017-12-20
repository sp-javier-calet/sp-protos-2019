using SocialPoint.IO;
using System.IO;
using System;
using SocialPoint.Base;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    public sealed class LocalBridgeNetworkMessage : ILocalNetworkMessage
    {
        public IWriter Writer { get; private set; }

        public NetworkMessageData Data { get; private set; }

        INetworkMessage _netMessage;
        ILocalNetworkMessage _localMessage;

        public LocalBridgeNetworkMessage(NetworkMessageData data, INetworkServer netServer, ILocalNetworkServer localServer)
        {
            Data = data;

            List<IWriter> messageWriters = new List<IWriter>();
            
            _netMessage = netServer.CreateMessage(data);
            messageWriters.Add(_netMessage.Writer);

            _localMessage = localServer.CreateLocalMessage(data);
            messageWriters.Add(_localMessage.Writer);

            Writer = new WriterGroup(messageWriters.ToArray());
        }

        public void Send()
        {
            if (_netMessage != null)
            {
                _netMessage.Send();
            }

            if (_localMessage != null)
            {
                _localMessage.Send();
            }
        }

        public IReader Receive()
        {
            if (_localMessage != null)
            {
                return _localMessage.Receive();
            }
            else
            {
                Log.e("Calling Receive on a LocalBridgeNetworkMessage that does not contain a LocalNetworkMessage. Returning null.");
                return null;
            }
        }
    }
}
