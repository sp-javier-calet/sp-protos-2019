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

        public UnetNetworkMessage(NetworkConnection[] conns, byte msgType, int channelId)
        {
            _channelId = channelId;
            _conns = conns;
            _writer = new NetworkWriter();
            _writer.StartMessage((short)(MsgType.Highest + 1 + msgType));
            Writer = new UnetNetworkWriter(_writer);
        }

        public static byte ConvertType(short type)
        {
            return (byte)(type - 1 - MsgType.Highest);
        }

        public void Send()
        {
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