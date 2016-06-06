using System;

public class GameModel : IDisposable
{
    public ConfigModel Config{ get; private set; }

    public PlayerModel Player{ get; private set; }

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