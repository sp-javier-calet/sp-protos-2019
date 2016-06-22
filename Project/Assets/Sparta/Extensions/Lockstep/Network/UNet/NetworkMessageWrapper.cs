using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using SocialPoint.Utils.Network.UNet;

namespace SocialPoint.Lockstep.Network.UNet
{
    public class NetworkMessageWrapper : MessageBase
    {
        public INetworkMessage NetworkMessage { get; private set; }

        public int NetworkTimestamp { get; private set; }

        public NetworkMessageWrapper(INetworkMessage networkMessage)
        {
            NetworkMessage = networkMessage;
        }

        public override void Deserialize(NetworkReader reader)
        {
            NetworkReaderWrapper readerWrapper = new NetworkReaderWrapper(reader);
            if(NetworkMessage.RequiresSync)
            {
                NetworkTimestamp = readerWrapper.ReadInt32();
            }
            NetworkMessage.Deserialize(readerWrapper);
        }

        public override void Serialize(NetworkWriter writer)
        {
            NetworkWriterWrapper writerWrapper = new NetworkWriterWrapper(writer);
            if(NetworkMessage.RequiresSync)
            {
                NetworkTimestamp = NetworkTransport.GetNetworkTimestamp();
                writerWrapper.Write(NetworkTimestamp);
            }
            NetworkMessage.Serialize(writerWrapper);
        }
    }
}