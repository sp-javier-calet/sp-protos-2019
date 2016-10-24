using System.IO;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    public class WebSocketSharpNetworkMessage : INetworkMessage
    {
        readonly NetworkMessageData _data;
        readonly SystemBinaryWriter _writer;
        readonly MemoryStream _stream;
        readonly WebSocketSharpClient _socket;

        public WebSocketSharpNetworkMessage(NetworkMessageData data, WebSocketSharpClient socket)
        {
            _data = data;
            _stream = new MemoryStream();
            _writer = new SystemBinaryWriter(_stream);
            _socket = socket;
        }

        #region INetworkMessage implementation

        public void Send()
        {
            _socket.SendNetworkMessage(_data, _stream.GetBuffer());
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
