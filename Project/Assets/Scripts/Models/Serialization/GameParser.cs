using SocialPoint.Attributes;

public class GameParser : IParser<GameModel>
{
    const string AttrKeyConfig = "config";
    const string AttrKeyUser = "user";
    const string AttrKeyConfigPatch = "config_patches";

    IParser<ConfigModel> _configParser;
    IParser<PlayerModel> _playerParser;
    IParser<ConfigPatch> _configPatchParser;

    public GameParser(IParser<ConfigModel> configParser, IParser<PlayerModel> playerParser, IParser<ConfigPatch> configPatchParser)
    {
        _configParser = configParser;
        _playerParser = playerParser;
        _configPatchParser = configPatchParser;
    }

    public GameModel Parse(Attr data)
    {
        var configPatch = _configPatchParser.Parse(data.AsDic[AttrKeyConfigPatch]);
        var patcher = new AttrPatcher();
        var configData = data.AsDic[AttrKeyConfig];
        patcher.Patch(configPatch.Patch,configData);
        var config = _configParser.Parse(configData);
        var player = _playerParser.Parse(data.AsDic[AttrKeyUser]);
        return new GameModel(config, player, configPatch);
    }
}