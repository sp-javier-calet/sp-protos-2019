using SocialPoint.Multiplayer;
using SocialPoint.IO;

public class ExplosionEvent
{
    public Vector3 Position;
}

public class ExplosionEventSerializer : SimpleSerializer<ExplosionEvent>
{
    Vector3Serializer _vec3 = new Vector3Serializer();

    public override void Serialize(ExplosionEvent newObj, IWriter writer)
    {
        _vec3.Serialize(newObj.Position, writer);
    }
}

public class ExplosionEventParser : SimpleParser<ExplosionEvent>
{
    Vector3Parser _vec3 = new Vector3Parser();

    public override ExplosionEvent Parse(IReader reader)
    {
        return new ExplosionEvent {
            Position = _vec3.Parse(reader)
        };
    }
}