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
    readonly string _jsonGameResource;
    readonly string _jsonPlayerResource;

    readonly IParser<GameModel> _gameParser;
    readonly IParser<PlayerModel> _playerParser;
    readonly IParser<ConfigModel> _configParser;
    readonly IParser<ConfigPatch> _configPatchParser;
    readonly ISerializer<PlayerModel> _playerSerializer;
    readonly GameModel _gameModel;
    readonly ILoginData _loginData;

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
            return _loginData == null || string.IsNullOrEmpty(_loginData.BaseUrl);
        }
    }

    public GameLoader(string jsonGameResource, string jsonPlayerResource, IParser<GameModel> gameParser, IParser<ConfigModel> configParser,
        IParser<PlayerModel> playerParser, IParser<ConfigPatch> configPatchParser, ISerializer<PlayerModel> playerSerializer, GameModel game, ILoginData loginData)
    {
        _jsonGameResource = jsonGameResource;
        _jsonPlayerResource = jsonPlayerResource;
        _gameParser = gameParser;
        _configParser = configParser;
        _playerParser = playerParser;
        _configPatchParser = configPatchParser;
        _playerSerializer = playerSerializer;
        _gameModel = game;
        _loginData = loginData;
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

        var configPatch = _configPatchParser.Parse(gameData.AsDic[GameParser.AttrKeyConfigPatch]);
        var configData = gameData.AsDic[GameParser.AttrKeyConfig];
        if(!new AttrPatcher().Patch(configPatch.Patch, configData))
        {
            configData = gameData.AsDic[GameParser.AttrKeyConfig];
        }
        return _configParser.Parse(configData);
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
            LoadConfigModel();
            var playerData = new JsonAttrParser().ParseString(json);
            _playerParser.Parse(playerData);

            return _gameModel;
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

        if(newModel == null)
        {
            throw new InvalidOperationException("Could not load the game.");
        }
        else
        {
            _gameModel.Init();
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
