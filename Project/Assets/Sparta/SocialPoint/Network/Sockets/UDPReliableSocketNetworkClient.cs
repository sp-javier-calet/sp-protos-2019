using System;

namespace SocialPoint.Network
{
    public sealed class UDPReliableSocketNetworkClient : SocketNetworkClient
    {
       
        public UDPReliableSocketNetworkClient(string serverAddr = null, int serverPort = UnetNetworkServer.DefaultPort) : base(serverAddr, serverPort)
        {
           
        }

        public override void Connect()
        {
            UnityEngine.Debug.Log("Connect");
            using(ENet.Host host = new ENet.Host())
            {
                UnityEngine.Debug.Log(String.Format("Initializing client..."));
                host.Initialize(null, 1);

                ENet.Peer peer = host.Connect("127.0.0.1", 5000, 1234, 200);
                while(true)
                {
                    ENet.Event @event;

                    if(host.Service(15000, out @event))
                    {
                        do
                        {
                            switch(@event.Type)
                            {
                            case ENet.EventType.Connect:
                                UnityEngine.Debug.Log(String.Format("Connected to server at IP/port {0}.", peer.GetRemoteAddress()));
                                break;

                            case ENet.EventType.Receive:
                                byte[] data = @event.Packet.GetBytes();
                                ushort value = BitConverter.ToUInt16(data, 0);
                                if(value % 1000 == 0)
                                {
                                    UnityEngine.Debug.Log(String.Format("  Client: Ch={0} Recv={1}", @event.ChannelID, value));
                                }
                                value++;
                                peer.Send(@event.ChannelID, BitConverter.GetBytes(value), ENet.PacketFlags.Reliable);
                                @event.Packet.Dispose();
                                break;

                            default:
                                UnityEngine.Debug.Log(@event.Type);
                                break;
                            }
                        }
                        while (host.CheckEvents(out @event));
                    }
                }
            }

        }

        public override void Disconnect()
        {
            UnityEngine.Debug.Log("Disconnect");
        }

        public override int GetDelay(int networkTimestamp)
        {
            throw new NotImplementedException();
        }

        public override byte ClientId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool Connected
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override INetworkMessage CreateMessage(NetworkMessageData data)
        {
            throw new NotImplementedException();
        }

      

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}