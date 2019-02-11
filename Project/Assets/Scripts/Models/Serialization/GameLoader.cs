using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Login;
using SocialPoint.Dependency;
using System;
using UnityEngine;

public interface IGameLoader
{
    GameModel Load(Attr data);

    void SaveLocalGame();

    void DeleteLocalGame();

    Attr OnAutoSync();
}

public class GameLoader : IGameLoader
{
    readonly TextAsset _jsonGameConfigResource;
    readonly TextAsset _jsonPlayerModelResource;

    readonly IAttrObjParser<GameModel> _gameParser;
    readonly IAttrObjParser<PlayerModel> _playerParser;
    readonly IAttrObjParser<ConfigModel> _configParser;
    readonly IAttrObjParser<ConfigPatch> _configPatchParser;
    readonly IAttrObjSerializer<PlayerModel> _playerSerializer;
    readonly GameModel _gameModel;
    readonly ILoginData _loginData;

    public string PlayerJsonPath
    {
        get
        {
            return string.Format("{0}/{1}.json", PathsManager.AppPersistentDataPath, _jsonPlayerModelResource ? _jsonPlayerModelResource.name : string.Empty);
        }
    }

    public bool IsLocalGame
    {
        get
        {
            return _loginData == null || string.IsNullOrEmpty(_loginData.BaseUrl);
        }
    }

    public GameLoader(TextAsset jsonGameConfigResource, TextAsset jsonPlayerModelResource, IAttrObjParser<GameModel> gameParser, IAttrObjParser<ConfigModel> configParser,
        IAttrObjParser<PlayerModel> playerParser, IAttrObjParser<ConfigPatch> configPatchParser, IAttrObjSerializer<PlayerModel> playerSerializer, GameModel game, ILoginData loginData)
    {
        _jsonGameConfigResource = jsonGameConfigResource;
        _jsonPlayerModelResource = jsonPlayerModelResource;
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
        if(_jsonGameConfigResource == null)
        {
            return null;
        }

        var json = _jsonGameConfigResource.text;
        var gameData = new JsonAttrParser().ParseString(json);
        return _gameParser.Parse(gameData);
    }

    ConfigModel LoadConfigModel()
    {
        if(_jsonGameConfigResource == null)
        {
            return null;
        }

        var json = _jsonGameConfigResource.text;
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
