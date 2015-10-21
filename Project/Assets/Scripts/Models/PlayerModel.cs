using System;

public class PlayerModel
{
    public long Level{ get; private set; }
        
    public PlayerModel(long level)
    {
        Level = level;
    }
    
    override public string ToString()
    {
        return string.Format("[PlayerModel: Level={0}]", Level);
    }
}