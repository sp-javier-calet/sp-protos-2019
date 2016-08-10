using UnityEngine.Networking;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    class UnetNetworkMessage : INetworkMessage
    {
        public IWriter Writer{ get; private set; }
        NetworkWriter _writer;
        NetworkConnection[] _conns;
        int _channelId;

        public UnetNetworkMessage(NetworkMessageInfo info, NetworkConnection[] conns)
        {
            _channelId = info.ChannelId;
            _conns = conns;
            _writer = new NetworkWriter();
            _writer.StartMessage((short)(MsgType.Highest + 1 + info.MessageType));
            Writer = new UnetNetworkWriter(_writer);
        }

        public static byte ConvertType(short type)
        {
            return (byte)(type - 1 - MsgType.Highest);
        }

        public void Send()
        {
            Writer = null;
            _writer.FinishMessage();
            for(var i = 0; i < _conns.Length; i++)
            {
                var conn = _conns[i];
                if(conn != null)
                {
                    conn.SendWriter(_writer, _channelId);
                }
            }
        }
    }
}