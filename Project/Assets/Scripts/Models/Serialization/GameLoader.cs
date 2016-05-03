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

    public GameLoader(string jsonGameResource, string jsonPlayerResource)
    {
        _jsonGameResource = jsonGameResource;
        _jsonPlayerResource = jsonPlayerResource;

        _gameParser = ServiceLocator.Instance.Resolve<IParser<GameModel>>();
        _playerParser = ServiceLocator.Instance.Resolve<IParser<PlayerModel>>();
        _playerSerializer = ServiceLocator.Instance.Resolve<ISerializer<PlayerModel>>();
        _gameModel = ServiceLocator.Instance.Resolve<GameModel>();
        _login = ServiceLocator.Instance.Resolve<ILogin>();
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
            var gameModel = LoadInitialGame();
            var playerData = new JsonAttrParser().ParseString(json);
            var player = _playerParser.Parse(playerData);

            gameModel.LoadPlayer(player);

            return gameModel;
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
                newModel.Player.Move(ini.Player);
            }
        }
        if(newModel == null)
        {
            throw new InvalidOperationException("Could not load the game.");
        }
        else
        {
            _gameModel.Move(newModel);
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
