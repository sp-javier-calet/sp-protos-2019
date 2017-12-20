using SocialPoint.IO;
using SocialPoint.Network;

namespace SocialPoint.WebSockets
{
    public class WebSocketNetworkMessage : INetworkMessage
    {
        readonly NetworkMessageData _data;
        readonly WebSocketsTextWriter _writer;
        readonly WebSocketClient _socket;

        public WebSocketNetworkMessage(NetworkMessageData data, WebSocketClient socket)
        {
            _data = data;
            _writer = new WebSocketsTextWriter();
            _socket = socket;
        }

        #region INetworkMessage implementation

        public void Send()
        {
            _socket.SendNetworkMessage(_data, _writer.ToString());
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