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

        public UnetNetworkMessage(NetworkMessageData data, NetworkConnection[] conns)
        {
            _channelId = data.ChannelId;
            _conns = conns;
            _writer = new NetworkWriter();
            _writer.StartMessage((short)(UnityEngine.Networking.MsgType.Highest + 1 + data.MessageType));
            Writer = new UnetNetworkWriter(_writer);
        }

        public static byte ConvertType(short type)
        {
            return (byte)(type - 1 - UnityEngine.Networking.MsgType.Highest);
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