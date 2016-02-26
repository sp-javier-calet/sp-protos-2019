using System;

public class GameModel : IDisposable
{
    public ConfigModel Config{ get; private set; }

    public PlayerModel Player{ get; private set; }

    public event Action<GameModel> Moved;

    public bool IsMoved{ get; private set; }

    public GameModel(ConfigModel config = null, PlayerModel player = null)
    {
        if(config == null)
        {
            config = new ConfigModel();
        }
        Config = config;
        if(player == null)
        {
            player = new PlayerModel();
        }
        Player = player;
    }

    public void Move(GameModel other)
    {
        IsMoved = true;
        if(Player != other.Player)
        {
            Player.Move(other.Player);
        }

        if(Config != other.Config)
        {
            Config.Move(other.Config);
        }

        other.Player = null;
        other.Config = null;
        other.Dispose();

        if(Moved != null)
        {
            Moved(this);
        }
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