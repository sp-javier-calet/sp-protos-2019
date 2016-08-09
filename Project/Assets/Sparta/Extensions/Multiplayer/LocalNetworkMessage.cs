
using SocialPoint.IO;
using System.IO;

namespace SocialPoint.Multiplayer
{
    public interface ILocalNetworkMessageReceiver
    {
        void OnLocalMessageReceived(LocalNetworkMessage msg);
    }

    public class LocalNetworkMessage : INetworkMessage
    {
        public IWriter Writer{ get; private set; }
        byte _type;
        int _channelId;

        ILocalNetworkMessageReceiver _receiver;
        MemoryStream _stream;

        public LocalNetworkMessage(byte type, int chanId, ILocalNetworkMessageReceiver receiver)
        {
            _receiver = receiver;
            _type = type;
            _channelId = chanId;
            _stream = new MemoryStream();
            Writer = new SystemBinaryWriter(_stream);
        }

        public void Send()
        {
            _receiver.OnLocalMessageReceived(this);
        }

        public ReceivedNetworkMessage Receive()
        {
            return new ReceivedNetworkMessage(_type, _channelId, new SystemBinaryReader(_stream));
        }
    }
}