using System;
using System.IO;
using System.Collections.Generic;
using SocialPoint.Network;
using SocialPoint.IO;
using Photon.Hive.Plugin;

namespace SocialPoint.Network
{
    public class PluginNetworkMessage : INetworkMessage
    {
        IPluginHost _host;
        List<int> _actors;
        byte _msgType;
        MemoryStream _stream;
        SystemBinaryWriter _writer;
        bool _unreliable;

        const byte EventDataKey = 245;
        const byte SenderActorKey = 254;

        public PluginNetworkMessage(IPluginHost host, byte msgType, bool unreliable, List<int> actors)
        {
            _host = host;
            _actors = actors;
            _msgType = msgType;
            _stream = new MemoryStream();
            _writer = new SystemBinaryWriter(_stream);
            _unreliable = unreliable;
        }

        public IWriter Writer
        {
            get
            {
                return _writer;
            }
        }

        public void Send()
        {
            var evData = _stream.ToArray();
            evData = HttpEncoding.Encode(evData, HttpEncoding.LZ4);

            var data = new Dictionary<byte, object> { { EventDataKey, evData } };
            var parms = new SendParameters();
            parms.Unreliable = _unreliable;
            _host.BroadcastEvent(
                _actors, 0, _msgType,
                data, 0, parms);
        }
    }
}