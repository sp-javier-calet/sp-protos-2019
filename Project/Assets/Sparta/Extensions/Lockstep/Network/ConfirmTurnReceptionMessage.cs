
using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public sealed class ConfirmTurnReceptionMessage : INetworkShareable
    {
        public int Turn { get; private set; }

        public ConfirmTurnReceptionMessage(int turn = 0)
        {
            Turn = turn;
        }

        public void Deserialize(IReader reader)
        {
            Turn = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Turn);
        }
    }
}