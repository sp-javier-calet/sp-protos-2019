using SocialPoint.Attributes;

public class GameParser : IAttrObjParser<GameModel>
{
    public const string AttrKeyConfig = "config";
    public const string AttrKeyUser = "user";
    public const string AttrKeyConfigPatch = "config_patches";

    IAttrObjParser<ConfigModel> _configParser;
    IAttrObjParser<PlayerModel> _playerParser;
    IAttrObjParser<ConfigPatch> _configPatchParser;
    GameModel _gameModel;

    public GameParser(GameModel gameModel, IAttrObjParser<ConfigModel> configParser, IAttrObjParser<PlayerModel> playerParser, IAttrObjParser<ConfigPatch> configPatchParser)
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