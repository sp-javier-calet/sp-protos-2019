
using SocialPoint.IO;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    public sealed class SimpleSocketNetworkMessage : INetworkMessage
    {
        readonly NetworkMessageData _data;
        readonly SystemBinaryWriter _writer;
        readonly MemoryStream _memStream;
        List<NetworkStream> _netStreams;


        public SimpleSocketNetworkMessage(NetworkMessageData data, List<NetworkStream> netStreams)
        {
            _data = data;
            _memStream = new MemoryStream();
            _netStreams = netStreams;
            _writer = new SystemBinaryWriter(_memStream);
        }

        public void Send()
        {
            _memStream.Seek(0, SeekOrigin.Begin);
            var data = _memStream.ToArray();
            for(int i = 0; i < _netStreams.Count; i++)
            {
                NetworkStream netStream = _netStreams[i];
                var netWriter = new SystemBinaryWriter(netStream);
                netWriter.Write(_data.MessageType);
                netWriter.Write(data.Length);
                netWriter.Write(data, data.Length);
                netWriter.Flush();
            }
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
