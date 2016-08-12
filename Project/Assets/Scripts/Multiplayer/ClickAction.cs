using SocialPoint.Multiplayer;
using SocialPoint.IO;

public class ClickAction
{
    public Vector3 Position;
}

public class ClickActionSerializer : SimpleSerializer<ClickAction>
{
    Vector3Serializer _vec3 = new Vector3Serializer();

    public override void Serialize(ClickAction newObj, IWriter writer)
    {
        _vec3.Serialize(newObj.Position, writer);
    }
}

public class ClickActionParser : SimpleParser<ClickAction>
{
    Vector3Parser _vec3 = new Vector3Parser();

    public override ClickAction Parse(IReader reader)
    {
        return new ClickAction {
            Position = _vec3.Parse(reader)
        };
    }
}