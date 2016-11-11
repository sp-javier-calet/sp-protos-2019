using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
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
            EmptyTurns = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(EmptyTurns);
        }
    }
}