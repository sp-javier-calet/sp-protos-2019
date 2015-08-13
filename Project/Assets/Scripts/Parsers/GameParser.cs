using SocialPoint.Attributes;

public class GameParser : IParser<GameModel>
{
    public GameModel Parse(Attr data)
    {
        var model = new GameModel();
        return model;
    }
}
