//
//using SocialPoint.IO;
//using System.IO;
//using System.Net.Sockets;
//
//namespace SocialPoint.Network
//{
//    public sealed class SimpleSocketNetworkMessage : INetworkMessage
//    {
//        readonly NetworkMessageData _data;
//        readonly SystemBinaryWriter _writer;
//        readonly NetworkStream _netStream;
//        readonly MemoryStream _memStream;
//
//
//        public SimpleSocketNetworkMessage(NetworkMessageData data, NetworkStream stream)
//        {
//            _data = data;
//            _netStream = stream;
//            _memStream = new MemoryStream();
//            _writer = new SystemBinaryWriter(_memStream);
//        }
//
//
//        public void Send()
//        {
//            _memStream.Seek(0, SeekOrigin.Begin);
//            var data = _memStream.ToArray();
//            var netWriter = new SystemBinaryWriter(_memStream);
//            netWriter.Write(_data.MessageType);
//            netWriter.Write(data.Length);
//            netWriter.Write(data, data.Length);
//            netWriter.Flush();
//        }
//
//        public IWriter Writer
//        {
//            get
//            {
//                return _writer;
//            }
//        }
//    }
//            
//}
