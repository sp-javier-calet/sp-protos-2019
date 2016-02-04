using System;

public class PlayerModel
{
    public long Level{ get; private set; }

    public ResourcePool Resources{ get; private set; }

    public event Action<PlayerModel> Assigned;

    public PlayerModel(long level = 0, ResourcePool resources = null)
    {
        Level = level;

        if(resources == null)
        {
            resources = new ResourcePool();
        }
        Resources = resources;
    }

    public void Assign(PlayerModel other)
    {
        Level = other.Level;
        Resources.Assign(other.Resources);
        if(Assigned != null)
        {
            Assigned(this);
        }
    }

    
    override public string ToString()
    {
        return string.Format("[PlayerModel: Level={0}, Resources={1}]", Level, Resources);
    }
}