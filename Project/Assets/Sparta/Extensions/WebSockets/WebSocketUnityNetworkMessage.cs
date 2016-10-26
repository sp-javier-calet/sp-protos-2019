using System.IO;
using SocialPoint.IO;
using SocialPoint.Network;

namespace SocialPoint.WebSockets
{
    public class WebSocketUnityNetworkMessage : INetworkMessage
    {
        readonly NetworkMessageData _data;
        readonly WebSocketsTextWriter _writer;
        readonly WebSocketUnityClient _socket;

        public WebSocketUnityNetworkMessage(NetworkMessageData data, WebSocketUnityClient socket)
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
