using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Login;

using Zenject;

public interface IGameLoader
{
    GameModel Load(Attr data);

    void SaveLocalGame();

    void DeleteLocalGame();

    Attr OnAutoSync();
}

public class GameLoader : IGameLoader
{
    string _jsonGameResource;
    string _jsonPlayerResource;

    [Inject]
    IParser<GameModel> _parser;

    [Inject]
    IParser<PlayerModel> _playerParser;

    [Inject]
    ISerializer<PlayerModel> _playerSerializer;

    [Inject]
    GameModel _model;

    [InjectOptional]
    ILogin _login;

    public GameLoader([Inject("game_initial_json_game_resource")] string jsonGameResource, [Inject("game_initial_json_player_resource")] string jsonPlayerResource)
    {
        _jsonGameResource = jsonGameResource;
        _jsonPlayerResource = jsonPlayerResource;
    }

    GameModel GetInitial()
    {
        var defaultGameJson = (UnityEngine.Resources.Load(_jsonGameResource) as UnityEngine.TextAsset).text;
        var gameData = new JsonAttrParser().ParseString(defaultGameJson).AsDic;
        return _parser.Parse(gameData);
    }

    GameModel LoadSavedGame()
    {
        var savedPlayerGameJson = UnityEngine.Resources.Load(_jsonPlayerResource) as UnityEngine.TextAsset;
        if(savedPlayerGameJson != null)
        {
            var playerData = new JsonAttrParser().ParseString(savedPlayerGameJson.text).AsDic;
            var initialGame = GetInitial();
            var savedPlayer = _playerParser.Parse(playerData);

            return new GameModel(initialGame.Config, savedPlayer);
        }
        return null;
    }

    public GameModel Load(Attr data)
    {
        //if there is no backend
        if(Attr.IsNullOrEmpty(data))
        {
            var savedGame = LoadSavedGame(); 
            if(savedGame != null)
            {
                _model.Assign(savedGame);
                return _model;
            }
            _model.Assign(GetInitial());

            return _model;
        }
        var newModel = _parser.Parse(data);

        if(newModel.Player == null)
        {
            var initialGame = GetInitial();
            newModel = new GameModel(newModel.Config, initialGame.Player);
        }
        data.Dispose();
        return _model;
    }

    string PlayerJsonPath()
    {
        return string.Format("{0}/{1}/{2}.json", PathsManager.DataPath, "Resources",
            _jsonPlayerResource);
    }

    public void SaveLocalGame()
    {
        Attr data = _playerSerializer.Serialize(_model.Player);
        IAttrSerializer Serializer = new JsonAttrSerializer();

        FileUtils.WriteAllText(PlayerJsonPath(), Serializer.SerializeString(data));
    }

    public void DeleteLocalGame()
    {
        FileUtils.Delete(PlayerJsonPath());
    }

    public Attr OnAutoSync()
    {
        if(_login == null || (_login.BaseUrl == null || _login.BaseUrl == string.Empty))
        {
            SaveLocalGame();
            return null;
        }
        if(_model == null || _model.Player == null)
        {
            return null;
        }
        return _playerSerializer.Serialize(_model.Player);

    }
}