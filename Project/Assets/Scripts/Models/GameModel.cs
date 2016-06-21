using System;

public class GameModel : IDisposable
{

    public ConfigModel Config{ get; private set; }

    public PlayerModel Player{ get; private set; }

    public event Action<GameModel> Initialized;

    public GameModel()
    {
        Config = new ConfigModel();
        Player = new PlayerModel();
    }

    public GameModel Init()
    {
        if(Initialized != null)
        {
            Initialized(this);
        }

        return this;
    }

    public void LoadPlayer(PlayerModel player)
    {
        Player.Dispose();
        Player = player;
    }

    public override string ToString()
    {
        return string.Format("[GameModel: Config={0}, Player={1}]", Config, Player);
    }

    public void Dispose()
    {
        if(Config != null)
        {
            Config.Dispose();
        }

        if(Player != null)
        {
            Player.Dispose();
        }
    }
}