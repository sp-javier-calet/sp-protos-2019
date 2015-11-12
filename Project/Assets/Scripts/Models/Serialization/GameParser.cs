using SocialPoint.Attributes;

public class GameParser : IParser<GameModel>
{
    const string AttrKeyConfig = "config";
    const string AttrKeyUser = "user";

    IParser<ConfigModel> _configParser;
    IParser<PlayerModel> _playerParser;

    public GameParser(IParser<ConfigModel> configParser, IParser<PlayerModel> playerParser)
    {
        _configParser = configParser;
        _playerParser = playerParser;
    }

    public GameModel Parse(Attr data)
    {
        var config = _configParser.Parse(data.AsDic[AttrKeyConfig]);
        var player = _playerParser.Parse(data.AsDic[AttrKeyUser]);
        return new GameModel(config, player);
    }
}