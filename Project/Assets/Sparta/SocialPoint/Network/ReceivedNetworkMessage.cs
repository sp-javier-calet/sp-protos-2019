
using SocialPoint.IO;

namespace SocialPoint.Network
{
    public class ReceivedNetworkMessage
    {
        public byte MessageType{ get; private set; }
        public byte ChannelId{ get; private set; }
        public IReader Reader{ get; private set; }

        public ReceivedNetworkMessage(byte type, byte chanId, IReader reader)
        {
            MessageType = type;
            ChannelId = chanId;
            Reader = reader;
        }
    }
}