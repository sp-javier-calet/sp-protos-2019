using System;

public class PlayerModel
{
    public long Level{ get; private set; }
        
    public PlayerModel(long level=0)
    {
        Level = level;
    }

    public void Assign(PlayerModel other)
    {
        Level = other.Level;           
    }

    
    override public string ToString()
    {
        return string.Format("[PlayerModel: Level={0}]", Level);
    }
}