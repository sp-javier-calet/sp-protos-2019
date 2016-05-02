using System;
using System.Text;
using UnityEngine.Networking;
using SocialPoint.Attributes;

namespace SocialPoint.Lockstep.Network
{
    public class AllClientsReadyMessage : MessageBase
    {
        public int NetworkTimestamp { get; private set; }

        long _timestamp;
        int _remainingMillisecondsToStart;

        public int GetRemaningMillisecondsToStart(int hostId, int connectionId)
        {
            byte error;
            int serverDelay = hostId != -1 ? NetworkTransport.GetRemoteDelayTimeMS(hostId, connectionId, NetworkTimestamp, out error) : 0;
            return (int)(_timestamp - SocialPoint.Utils.TimeUtils.TimestampMilliseconds) + _remainingMillisecondsToStart - serverDelay;
        }

        public AllClientsReadyMessage(int remainingMillisecondsToStart = 0)
        {
            _timestamp = SocialPoint.Utils.TimeUtils.TimestampMilliseconds;
            _remainingMillisecondsToStart = remainingMillisecondsToStart;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            NetworkTimestamp = reader.ReadInt32();
            _timestamp = SocialPoint.Utils.TimeUtils.TimestampMilliseconds;
            _remainingMillisecondsToStart = reader.ReadInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(NetworkTransport.GetNetworkTimestamp());
            writer.Write(_remainingMillisecondsToStart);
        }
    }
}