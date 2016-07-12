using System;

public class PlayerModel : IDisposable
{
    public long Level{ get; private set; }

    public ResourcePool Resources{ get; private set; }

    public GoalsModel Goals { get; private set; }

    public event Action<PlayerModel> Initialized;

    public PlayerModel()
    {
        Goals = new GoalsModel();
    }

    public PlayerModel Init(long level, ResourcePool resources)
    {
        Level = level;

        Resources = resources;

        if(Initialized != null)
        {
            Initialized(this);
        }

        return this;
    }

    public override string ToString()
    {
        return string.Format("[PlayerModel: Level={0}, Resources={1}, Goals={2}]", Level, Resources, Goals);
    }

    public void Dispose()
    {
        if(Goals != null)
        {
            Goals.Dispose();
        }
    }
}