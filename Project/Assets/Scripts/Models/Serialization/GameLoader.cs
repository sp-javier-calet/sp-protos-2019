using SocialPoint.Attributes;
using Zenject;

public interface IGameLoader
{
    GameModel LoadInitial();

    GameModel Load(Attr data);
}

public class GameLoader : IGameLoader
{
    string _jsonGameResource;
    string _jsonPlayerResource;

    [Inject]
    IParser<GameModel> _parser;

    [Inject]
    GameModel _model;

    public GameLoader([Inject("game_initial_json_game_resource")] string jsonGameResource, [Inject("game_initial_json_player_resource")] string jsonPlayerResource)
    {
        _jsonGameResource = jsonGameResource;
        _jsonPlayerResource = jsonPlayerResource;
    }

    GameModel GetInitial()
    {
        var defaultGameJson = (UnityEngine.Resources.Load(_jsonGameResource) as UnityEngine.TextAsset).text;
        var gameData = new JsonAttrParser().ParseString(defaultGameJson).AsDic;

        var defaultPlayerGameJson = UnityEngine.Resources.Load(_jsonPlayerResource) as UnityEngine.TextAsset;
        if(defaultPlayerGameJson != null)
        {
            var playerData = new JsonAttrParser().ParseString(defaultPlayerGameJson.text).AsDic;

            gameData.Set("user", playerData);
        }
            
        return _parser.Parse(gameData);
    }

    public GameModel LoadInitial()
    {
        _model.Assign(GetInitial());
       
        return _model;
    }

    public GameModel Load(Attr data)
    {
        if(Attr.IsNullOrEmpty(data))
        {
            return LoadInitial();
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
}