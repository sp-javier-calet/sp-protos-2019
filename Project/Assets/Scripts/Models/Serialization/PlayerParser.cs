using SocialPoint.Attributes;

public class PlayerParser : IParser<PlayerModel>, ISerializer<PlayerModel>
{
    const string AttrKeyLevel = "level";
    const string AttrKeyResourcePool = "resources";

    public PlayerParser(ConfigModel config)
    {
    }

    public PlayerModel Parse(Attr data)
    {
        var level = data.AsDic[AttrKeyLevel].AsValue.ToInt();
        var respoolParser = new ResourcePoolParser();
        var resources = respoolParser.Parse(data.AsDic[AttrKeyResourcePool]);
        return new PlayerModel(level, resources);
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