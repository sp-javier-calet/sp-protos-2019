
using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public sealed class ConfirmTurnsReceptionMessage : INetworkShareable
    {
        public int[] ConfirmedTurns { get; private set; }

        public ConfirmTurnsReceptionMessage(int[] confirmedTurns = null)
        {
            ConfirmedTurns = confirmedTurns;
        }

        public void Deserialize(IReader reader)
        {
            int length = (int)reader.ReadByte();
            ConfirmedTurns = new int[length];
            for(int i = 0; i < length; ++i)
            {
                ConfirmedTurns[i] = reader.ReadInt32();
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write((byte)ConfirmedTurns.Length);
            for(int i = 0; i < ConfirmedTurns.Length; ++i)
            {
                writer.Write(ConfirmedTurns[i]);
            }
        }
    }
}