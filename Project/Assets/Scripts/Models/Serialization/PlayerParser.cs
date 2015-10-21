using SocialPoint.Attributes;

public class PlayerParser : IParser<PlayerModel>, ISerializer<PlayerModel>
{
    const string AttrKeyLevel = "level";

    public PlayerParser(ConfigModel config)
    {
    }

    public PlayerModel Parse(Attr data)
    {
        var level = data.AsDic[AttrKeyLevel].AsValue.ToInt();
        return new PlayerModel(level);
    }

    public Attr Serialize(PlayerModel player)
    {
        var data = new AttrDic();
        data.SetValue(AttrKeyLevel, player.Level);
        return data;
    }
}