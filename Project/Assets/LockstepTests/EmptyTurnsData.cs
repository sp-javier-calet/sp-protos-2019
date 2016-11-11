using SocialPoint.Multiplayer;
using SocialPoint.IO;
using Jitter.LinearMath;

public class EmptyTurnsData : INetworkShareable
{
    public int EmptyTurns = 1;

    public EmptyTurnsData(int emptyTurns = 0)
    {
        EmptyTurns = emptyTurns;
    }

    public void Deserialize(IReader reader)
    {
        EmptyTurns = (int) reader.ReadUInt32();
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(EmptyTurns);
    }
}
