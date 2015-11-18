using SocialPoint.Attributes;
using Zenject;

public interface IGameLoader
{
    GameModel LoadInitial();
    GameModel Load(Attr data);
}

public class GameLoader : IGameLoader
{
    string _jsonResource;

    [Inject]
    IParser<GameModel> _parser;

    [Inject]
    GameModel _model;

    public GameLoader([Inject("game_initial_json_resource")] string jsonResource)
    {
        _jsonResource = jsonResource;
    }

    GameModel GetInitial()
    {
        var defaultGame = (UnityEngine.Resources.Load(_jsonResource) as UnityEngine.TextAsset).text;
        var json = new JsonAttrParser().ParseString(defaultGame).AsDic;
        return _parser.Parse(json);
    }

    public GameModel LoadInitial()
    {
        if(_model.Config == null)
        {
            _model.Assign(GetInitial());
        }
        return _model;
    }

    public GameModel Load(Attr data)
    {
        var newModel = _parser.Parse(data);
        if(newModel.Player == null)
        {
            var initialGame = GetInitial();
            newModel = new GameModel(newModel.Config, initialGame.Player);
        }
        _model.Assign(newModel);
        data.Dispose();
        return _model;
    }
}