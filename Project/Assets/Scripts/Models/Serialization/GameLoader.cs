using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Login;
using System;
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
    IParser<GameModel> _gameParser;

    [Inject]
    IParser<PlayerModel> _playerParser;

    [Inject]
    ISerializer<PlayerModel> _playerSerializer;

    [Inject]
    GameModel _gameModel;

    [InjectOptional]
    ILogin _login;

    public string PlayerJsonPath
    {
        get
        {
            return string.Format("{0}/{1}.json", PathsManager.PersistentDataPath, _jsonPlayerResource);
        }
    }

    public bool IsLocalGame
    {
        get
        {
            return _login == null || string.IsNullOrEmpty(_login.BaseUrl);
        }
    }

    public GameLoader([Inject("game_initial_json_game_resource")] string jsonGameResource, [Inject("game_initial_json_player_resource")] string jsonPlayerResource)
    {
        _jsonGameResource = jsonGameResource;
        _jsonPlayerResource = jsonPlayerResource;
    }

    GameModel LoadInitialGame()
    {
        var json = (UnityEngine.Resources.Load(_jsonGameResource) as UnityEngine.TextAsset).text;
        var gameData = new JsonAttrParser().ParseString(json);
        return _gameParser.Parse(gameData);
    }

    GameModel LoadSavedGame()
    {
        string json = null;
        if(FileUtils.ExistsFile(PlayerJsonPath))
        {
            json = FileUtils.ReadAllText(PlayerJsonPath);
        }

        if(!string.IsNullOrEmpty(json))
        {
            var ini = LoadInitialGame();
            // we need to assign it or else the player parser will have the wrong config
            _gameModel.Assign(ini); 
            var playerData = new JsonAttrParser().ParseString(json);
            var player = _playerParser.Parse(playerData);
            return new GameModel(ini.Config, player);
        }
        return null;
    }

    public GameModel Load(Attr data)
    {
        GameModel newModel = null;
        if(data != null)
        {
            newModel = _gameParser.Parse(data);
            data.Dispose();
        }
        if(newModel == null && IsLocalGame)
        {
            newModel = LoadSavedGame();
        }
        if(newModel == null)
        {
            newModel = LoadInitialGame();
        }
        if(newModel != null && newModel.Player == null)
        {
            var ini = LoadInitialGame();
            if(ini != null)
            {
                newModel.Player.Assign(ini.Player);
            }
        }
        if(newModel == null)
        {
            throw new InvalidOperationException("Could not load the game.");
        }
        else
        {
            _gameModel.Assign(newModel);
        }
        return _gameModel;
    }

    public void SaveLocalGame()
    {
        Attr data = _playerSerializer.Serialize(_gameModel.Player);
        IAttrSerializer Serializer = new JsonAttrSerializer();

        FileUtils.WriteAllText(PlayerJsonPath, Serializer.SerializeString(data));
    }

    public void DeleteLocalGame()
    {
        FileUtils.DeleteFile(PlayerJsonPath);
        if(IsLocalGame)
        {
            Load(null);
        }
    }

    public Attr OnAutoSync()
    {
        if(IsLocalGame)
        {
            SaveLocalGame();
            return null;
        }
        if(_gameModel == null || _gameModel.Player == null)
        {
            return null;
        }
        return _playerSerializer.Serialize(_gameModel.Player);
    }
}
