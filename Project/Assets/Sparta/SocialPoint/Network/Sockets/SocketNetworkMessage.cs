using System;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    public static class SocketMsgType
    {
        public const short Fail = UnityEngine.Networking.MsgType.Highest + 1;
        public const short Highest = Fail;

        public static byte ConvertType(short type)
        {
            return (byte)(type - 1 - UnetMsgType.Highest);
        }
    }

    class SocketNetworkMessage : INetworkMessage
    {
        #region INetworkMessage implementation
        public void Send()
        {
            throw new NotImplementedException();
        }
        public IWriter Writer
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        #endregion
//        public IWriter Writer{ get; private set; }
//
//        NetworkWriter _writer;
//        NetworkConnection[] _conns;
//        int _channelId;
//
//        public EnetNetworkMessage(NetworkMessageData data, NetworkConnection[] conns)
//        {
//            if(data.MessageType > byte.MaxValue - UnetMsgType.Highest)
//            {
//                throw new ArgumentException("Message type is too big.");
//            }
//            _channelId = data.Unreliable ? Channels.DefaultUnreliable : Channels.DefaultReliable;
//            _conns = conns;
//            _writer = new NetworkWriter();
//            _writer.StartMessage((short)(UnetMsgType.Highest + 1 + data.MessageType));
//            Writer = new UnetNetworkWriter(_writer);
//        }
//
//        public void Send()
//        {
//            Writer = null;
//            _writer.FinishMessage();
//            for(var i = 0; i < _conns.Length; i++)
//            {
//                var conn = _conns[i];
//                if(conn != null)
//                {
//                    conn.SendWriter(_writer, _channelId);
//                }
//            }
//        }
    }
}