
using SocialPoint.IO;
using System.IO;

namespace SocialPoint.Network
{
    public sealed class SocketNetworkMessage : INetworkMessage
    {
        public IWriter Writer{ get; private set; }

        public NetworkMessageData Data{ get; private set; }

//        SocketNetworkServer _server;
//        SocketNetworkClient[] _clients;
//        SocketNetworkClient _origin;
        MemoryStream _stream;

        public SocketNetworkMessage(NetworkMessageData data, SocketNetworkClient[] clients)
        {
//            _clients = clients;
            Init(data);
        }

        public SocketNetworkMessage(NetworkMessageData data, SocketNetworkClient origin, SocketNetworkServer server)
        {
//            _origin = origin;
//            _server = server;
            Init(data);
        }

        void Init(NetworkMessageData data)
        {
            Data = data;
            _stream = new MemoryStream();
            Writer = new SystemBinaryWriter(_stream);
        }

        public void Send()
        {
//            UnityEngine.Debug.Log("SocketNetworkMessage Send");
            Writer.Write("test");
           
        }

        public IReader Receive()
        {
            var streamArray = _stream.ToArray();
            var data = new NetworkMessageData
            {
                ClientIds = Data.ClientIds,
                MessageType = Data.MessageType,
                Unreliable = Data.Unreliable,
                MessageLength = streamArray.Length
            };
            Data = data;
            return new SystemBinaryReader(new MemoryStream(streamArray));
        }
    }
}
