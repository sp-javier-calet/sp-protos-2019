using SocialPoint.Attributes;

public class PlayerParser : IParser<PlayerModel>
{
    public PlayerModel Parse(Attr data)
    {
        var model = new PlayerModel();
        return model;
    }
}