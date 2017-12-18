
using SocialPoint.IO;
using System.IO;
using System.Net.Sockets;

namespace SocialPoint.Network
{
    public sealed class SimpleSocketNetworkMessage : INetworkMessage
    {
        readonly NetworkMessageData _data;
        readonly SystemBinaryWriter _writer;
        readonly MemoryStream _memStream;
        readonly NetworkStream _netStream;


        public SimpleSocketNetworkMessage(NetworkMessageData data, NetworkStream netStream)
        {
            _data = data;
            _memStream = new MemoryStream();
            _netStream = netStream;
            _writer = new SystemBinaryWriter(_memStream);
        }


        public void Send()
        {
            _memStream.Seek(0, SeekOrigin.Begin);
            var data = _memStream.ToArray();
            var netWriter = new SystemBinaryWriter(_netStream);
            netWriter.Write(_data.MessageType);
            netWriter.Write(data.Length);
            netWriter.Write(data, data.Length);
            netWriter.Flush();
        }

        public IWriter Writer
        {
            get
            {
                return _writer;
            }
        }
    }
            
}
