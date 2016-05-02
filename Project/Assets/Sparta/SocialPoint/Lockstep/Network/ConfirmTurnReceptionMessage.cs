using System;
using UnityEngine.Networking;

namespace SocialPoint.Lockstep.Network
{
    public class ConfirmTurnsReceptionMessage : MessageBase
    {
        public int[] ConfirmedTurns { get; private set; }

        public int Client;

        public ConfirmTurnsReceptionMessage(int[] confirmedTurns = null)
        {
            ConfirmedTurns = confirmedTurns;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            int length = (int)reader.ReadByte();

            ConfirmedTurns = new int[length];
            for(int i = 0; i < length; ++i)
            {
                ConfirmedTurns[i] = reader.ReadInt32();
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.Write((byte)ConfirmedTurns.Length);
            for(int i = 0; i < ConfirmedTurns.Length; ++i)
            {
                writer.Write(ConfirmedTurns[i]);
            }
        }
    }
}