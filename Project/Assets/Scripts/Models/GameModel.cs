
using System;

public class GameModel
{
    public ConfigModel Config{ get; private set; }
    public PlayerModel Player{ get; private set; }
    
    public event Action Assigned;

    public GameModel(ConfigModel config=null, PlayerModel player=null)
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
    
    public void Assign(GameModel other)
    {
        Player.Assign(other.Player);
        Config.Assign(other.Config);
        
        if(Assigned != null)
        {
            Assigned();
        }              
    }
    
    public override string ToString()
    {
        return string.Format("[GameModel: Config={0}, Player={1}]", Config, Player);
    }
}