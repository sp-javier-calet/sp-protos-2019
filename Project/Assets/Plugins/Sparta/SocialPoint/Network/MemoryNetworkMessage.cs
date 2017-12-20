using SocialPoint.IO;
using System.IO;

namespace SocialPoint.Network
{
    public interface IMemoryNetworkMessageReceiver
    {
        void OnMessageSent(NetworkMessageData data, byte[] body);
    }

    class MemoryNetworkMessage : INetworkMessage
    {
        IMemoryNetworkMessageReceiver _receiver;
        NetworkMessageData _data;
        MemoryStream _stream;

        public IWriter Writer{ get; private set; }

        public MemoryNetworkMessage(NetworkMessageData data, IMemoryNetworkMessageReceiver receiver)
        {
            _data = data;
            _receiver = receiver;
            _stream = new MemoryStream();
            Writer = new SystemBinaryWriter(_stream);
        }

        public void Send()
        {
            _receiver.OnMessageSent(_data, _stream.ToArray());
        }
    }
}
