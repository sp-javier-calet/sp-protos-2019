using SocialPoint.Attributes;

public class GameParser : IParser<GameModel>
{
    const string AttrKeyConfig = "config";
    const string AttrKeyUser = "user";

    public GameModel Parse(Attr data)
    {
        var configParser = new ConfigParser();
        var config = configParser.Parse(data.AsDic[AttrKeyConfig]);
        var playerParser = new PlayerParser(config);
        var player = playerParser.Parse(data.AsDic[AttrKeyUser]);
        return new GameModel(config, player);
    }
}