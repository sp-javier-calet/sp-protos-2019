
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public class ReceivedNetworkMessage
    {
        public byte MessageType{ get; private set; }
        public int ChannelId{ get; private set; }
        public IReader Reader{ get; private set; }

        public ReceivedNetworkMessage(byte type, int chanId, IReader reader)
        {
            MessageType = type;
            ChannelId = chanId;
            Reader = reader;
        }
    }
}