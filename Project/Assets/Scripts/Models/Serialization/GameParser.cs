using SocialPoint.Attributes;

public class GameParser : IParser<GameModel>
{
    const string AttrKeyConfig = "config";
    const string AttrKeyUser = "user";

    IParser<ConfigModel> _configParser;
    IParser<PlayerModel> _playerParser;
    GameModel _gameModel;

    public GameParser(GameModel gameModel, IParser<ConfigModel> configParser, IParser<PlayerModel> playerParser)
    {
        _configParser = configParser;
        _playerParser = playerParser;
        _gameModel = gameModel;
    }

    public GameModel Parse(Attr data)
    {
        _configParser.Parse(data.AsDic[AttrKeyConfig]);
        _playerParser.Parse(data.AsDic[AttrKeyUser]);
        return _gameModel;
    }
}