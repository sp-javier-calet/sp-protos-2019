using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public sealed class EmptyTurnsMessage : INetworkShareable
    {
        public int EmptyTurns { get; private set; }

        public EmptyTurnsMessage(int emptyTurns = 0)
        {
            EmptyTurns = emptyTurns;
        }

        public void Deserialize(IReader reader)
        {
            EmptyTurns = (int) reader.ReadByte();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write((byte) EmptyTurns);
        }
    }
}