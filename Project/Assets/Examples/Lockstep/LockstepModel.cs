using SocialPoint.Utils;
using System;
using FixMath.NET;

public class LockstepModel
{
    long _mana = 0;
    int _nextObjectId = 0;
    long _duration;
    long _time;

    const long ManaSpeed = 2;
    const long MaxMana = 10000;

    const long UnitCost = 2000;

    public event Action<Fix64, Fix64, Fix64> OnInstantiate;
    public event Action OnFinish;

    public float ManaView
    {
        get
        {
            return ((float)_mana) / (float)MaxMana;
        }
    }

    public LockstepModel(long duration)
    {
        _duration = duration;
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

    public void Simulate(long dt)
    {
        _time += dt;
        if(_time > _duration)
        {
            if(OnFinish != null)
            {
                OnFinish();
            }
            return;
        }
        _mana += dt * ManaSpeed;
        if(_mana > MaxMana)
        {
            _mana = MaxMana;
        }
    }

    public void Reset()
    {
        _mana = 0;
        _time = 0;
    }
}
