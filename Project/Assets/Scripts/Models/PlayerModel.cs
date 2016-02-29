using System;

public class PlayerModel : IDisposable
{
    public long Level{ get; private set; }

    public ResourcePool Resources{ get; private set; }

    public event Action<PlayerModel> Moved;

    public PlayerModel(long level = 0, ResourcePool resources = null)
    {
        Level = level;

        if(resources == null)
        {
            resources = new ResourcePool();
        }
        Resources = resources;
    }

    public void Move(PlayerModel other)
    {
        Level = other.Level;
        Resources.Assign(other.Resources);

        other.Resources = null;
        other.Dispose();

        if(Moved != null)
        {
            Moved(this);
        }
    }

    
    override public string ToString()
    {
        return string.Format("[PlayerModel: Level={0}, Resources={1}]", Level, Resources);
    }

    public void Dispose()
    {
        
    }
}