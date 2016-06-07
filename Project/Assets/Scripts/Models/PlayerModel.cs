using System;

public class PlayerModel : IDisposable
{
    public long Level{ get; private set; }

    public ResourcePool Resources{ get; private set; }

    public PlayerModel Init(long level, ResourcePool resources)
    {
        Level = level;

        Resources = resources;

        return this;
    }

    override public string ToString()
    {
        return string.Format("[PlayerModel: Level={0}, Resources={1}]", Level, Resources);
    }

    public void Dispose()
    {
        
    }
}