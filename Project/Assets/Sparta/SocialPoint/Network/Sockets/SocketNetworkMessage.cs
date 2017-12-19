
using SocialPoint.IO;
using System.IO;
using System.Net.Sockets;

namespace SocialPoint.Network
{
    public sealed class SocketNetworkMessage : INetworkMessage
    {
//        readonly NetworkMessageData _data;
        readonly SocketNetworkWriter _writer;
//        readonly SimpleSocketNetworkClient _client;

        public SocketNetworkMessage(NetworkMessageData data, SimpleSocketNetworkClient client)
        {
//            _data = data;
            _writer = new SocketNetworkWriter();
//            _client = client;
        }

        #region INetworkMessage implementation

        public void Send()
        {
        }

        public IWriter Writer
        {
            get
            {
                return _writer;
            }
        }

        #endregion
    }
}
