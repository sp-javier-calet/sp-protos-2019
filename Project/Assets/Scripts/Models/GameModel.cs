
using System;

public class GameModel
{
    public ConfigModel Config{ get; private set; }
    public PlayerModel Player{ get; private set; }
    
    public event Action Assigned;

    public GameModel(ConfigModel config=null, PlayerModel player=null)
    {
        Config = config;
        Player = player;
    }
    
    public void Assign(GameModel other)
    {
        Player = other.Player;
        Config = other.Config;
        
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