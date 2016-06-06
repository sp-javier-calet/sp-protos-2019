using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Login;
using SocialPoint.Dependency;
using System;

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

    IParser<GameModel> _gameParser;
    IParser<PlayerModel> _playerParser;
    IParser<ConfigModel> _configParser;
    ISerializer<PlayerModel> _playerSerializer;
    GameModel _gameModel;
    ILogin _login;

    public string PlayerJsonPath
    {
        get
        {
            return string.Format("{0}/{1}.json", PathsManager.AppPersistentDataPath, _jsonPlayerResource);
        }
    }

    public bool IsLocalGame
    {
        get
        {
            return _login == null || string.IsNullOrEmpty(_login.BaseUrl);
        }
    }

    public GameLoader(string jsonGameResource, string jsonPlayerResource, IParser<GameModel> gameParser, IParser<ConfigModel> configParser,
                      IParser<PlayerModel> playerParser, ISerializer<PlayerModel> playerSerializer, GameModel game, ILogin login)
    {
        _jsonGameResource = jsonGameResource;
        _jsonPlayerResource = jsonPlayerResource;
        _gameParser = gameParser;
        _configParser = configParser;
        _playerParser = playerParser;
        _playerSerializer = playerSerializer;
        _gameModel = game;
        _login = login;
    }

    GameModel LoadInitialGame()
    {
        var json = (UnityEngine.Resources.Load(_jsonGameResource) as UnityEngine.TextAsset).text;
        var gameData = new JsonAttrParser().ParseString(json);
        return _gameParser.Parse(gameData);
    }

    ConfigModel LoadConfigModel()
    {
        var json = (UnityEngine.Resources.Load(_jsonGameResource) as UnityEngine.TextAsset).text;
        var gameData = new JsonAttrParser().ParseString(json);
        return _configParser.Parse(gameData.AsDic["config"]);
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
            var gameConfig = LoadConfigModel();
            var playerData = new JsonAttrParser().ParseString(json);
            var player = _playerParser.Parse(playerData);

            var gameModel = new GameModel(gameConfig, player);

            return gameModel;
        }
        return null;
    }

    public GameModel Load(Attr data)
    {
        if(data != null)
        {
            _gameModel = _gameParser.Parse(data);
            data.Dispose();
        }

        if(_gameModel == null && IsLocalGame)
        {
            _gameModel = LoadSavedGame();
        }

        if(_gameModel == null)
        {
            _gameModel = LoadInitialGame();
        }

        if(_gameModel == null)
        {
            throw new InvalidOperationException("Could not load the game.");
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
