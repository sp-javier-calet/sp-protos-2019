using SocialPoint.Attributes;

public class GameParser : IParser<GameModel>
{
    public const string AttrKeyConfig = "config";
    public const string AttrKeyUser = "user";
    public const string AttrKeyConfigPatch = "config_patches";

    IParser<ConfigModel> _configParser;
    IParser<PlayerModel> _playerParser;
    IParser<ConfigPatch> _configPatchParser;
    GameModel _gameModel;

    public GameParser(GameModel gameModel, IParser<ConfigModel> configParser, IParser<PlayerModel> playerParser, IParser<ConfigPatch> configPatchParser)
    {
        _configParser = configParser;
        _playerParser = playerParser;
        _configPatchParser = configPatchParser;
        _gameModel = gameModel;
    }

    public GameModel Parse(Attr data)
    {
        var configPatch = _configPatchParser.Parse(data.AsDic[AttrKeyConfigPatch]);
        var configData = data.AsDic[AttrKeyConfig];
        if(!new AttrPatcher().Patch(configPatch.Patch, configData))
        {
            configData = data.AsDic[AttrKeyConfig];
        }
        _configParser.Parse(configData);
        _playerParser.Parse(data.AsDic[AttrKeyUser]);
        return _gameModel;
    }
}