using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;

public class PlayerParser : IAttrObjParser<PlayerModel>, IAttrObjSerializer<PlayerModel>
{
    const string AttrKeyLevel = "level";
    const string AttrKeyResourcePool = "resources";
    const string AttrKeyGoals = "goals";

    PlayerModel _playerModel;
    GoalsModelParser _goalsParser;

    public PlayerParser(PlayerModel playerModel, ConfigModel config, IScriptEventProcessor scriptEventDispatcher)
    {
        _playerModel = playerModel;
        _goalsParser = new GoalsModelParser(_playerModel.Goals, config, scriptEventDispatcher, _playerModel);
    }

    public PlayerModel Parse(Attr data)
    {
        var level = data.AsDic[AttrKeyLevel].AsValue.ToInt();
        var respoolParser = new ResourcePoolParser();
        var resources = respoolParser.Parse(data.AsDic[AttrKeyResourcePool]);
        _goalsParser.Parse(data.AsDic[AttrKeyGoals]);

        return _playerModel.Init(level, resources);
    }

    public Attr Serialize(PlayerModel player)
    {
        var attrDic = new AttrDic();
        attrDic.Set(AttrKeyLevel, new AttrLong(player.Level));
        var respoolParser = new ResourcePoolParser();
        attrDic.Set(AttrKeyResourcePool, respoolParser.Serialize(player.Resources));
        return attrDic;
    }
}