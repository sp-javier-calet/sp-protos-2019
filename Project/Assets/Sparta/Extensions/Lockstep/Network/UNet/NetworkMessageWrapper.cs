using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using SocialPoint.Network;

namespace SocialPoint.Lockstep.Network.UNet
{
    public sealed class NetworkMessageWrapper : MessageBase
    {
        public INetworkMessage NetworkMessage { get; private set; }

        public int NetworkTimestamp { get; private set; }

        public NetworkMessageWrapper(INetworkMessage networkMessage)
        {
            NetworkMessage = networkMessage;
        }

        public override void Deserialize(NetworkReader reader)
        {
            UnetNetworkReader readerWrapper = new UnetNetworkReader(reader);
            if(NetworkMessage.RequiresSync)
            {
                NetworkTimestamp = readerWrapper.ReadInt32();
            }
            NetworkMessage.Deserialize(readerWrapper);
        }

        public override void Serialize(NetworkWriter writer)
        {
            UnetNetworkWriter writerWrapper = new UnetNetworkWriter(writer);
            if(NetworkMessage.RequiresSync)
            {
                NetworkTimestamp = NetworkTransport.GetNetworkTimestamp();
                writerWrapper.Write(NetworkTimestamp);
            }
            NetworkMessage.Serialize(writerWrapper);
        }
    }
}