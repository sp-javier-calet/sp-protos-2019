using SocialPoint.Utils;
using System;
using FixMath.NET;

public class LockstepModel : ISimulateable
{
    long _mana = 0;
    long _lastTimestamp;
    int _nextObjectId = 0;

    const long ManaSpeed = 2;
    const long MaxMana = 10000;

    const long UnitCost = 7000;

    public event Action<Fix64, Fix64, Fix64> OnInstantiate;

    public float ManaView
    {
        get
        {
            return ((float)_mana) / (float)MaxMana;
        }
    }

    public bool OnClick(Fix64 x, Fix64 y, Fix64 z)
    {
        if(_mana > UnitCost)
        {
            if(OnInstantiate != null)
            {
                OnInstantiate(x, y, z);
            }
            _nextObjectId++;
            _mana -= UnitCost;
        }
        return true;
    }

    public void Simulate(long timestamp)
    {
        var dt = timestamp - _lastTimestamp;
        _mana += dt * ManaSpeed;
        if(_mana > MaxMana)
        {
            _mana = MaxMana;
        }
        _lastTimestamp = timestamp;
    }

    public long KeyTimestamp
    {
        get
        {
            return 0;
        }
    }
}
