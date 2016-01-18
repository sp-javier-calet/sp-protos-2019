using SocialPoint.Attributes;
using SocialPoint.IO;
using Zenject;

public interface IPlayerSaver
{
    void Save(PlayerModel player);
}

public class PlayerSaver : IPlayerSaver
{
    [Inject]
    ISerializer<PlayerModel> _playerParser;

    string _jsonPlayerResource;

    public string PlayerJsonPath()
    {
        return string.Format("{0}/{1}/{2}.json", PathsManager.DataPath, "Resources",
            _jsonPlayerResource);
    }

    public PlayerSaver([Inject("game_initial_json_player_resource")] string jsonPlayerResource)
    {
        _jsonPlayerResource = jsonPlayerResource;
    }

    public void Save(PlayerModel player)
    {
        Attr data = _playerParser.Serialize(player);
        IAttrSerializer Serializer = new JsonAttrSerializer();

        FileUtils.WriteAllText(PlayerJsonPath(), Serializer.SerializeString(data));

        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
}
