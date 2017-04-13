using System;
using FixMath.NET;
using SocialPoint.Attributes;
using System.Collections.Generic;

namespace Examples.Lockstep
{
    [Serializable]
    public class Config
    {
        const long DefaultManaSpeed = 2;
        const long DefaultMaxMana = 10000;
        const long DefaultGameDuration = 30000;
        const long DefaultUnitCost = 2000;

        public long ManaSpeed = DefaultManaSpeed;
        public long MaxMana = DefaultMaxMana;
        public long Duration = DefaultGameDuration;
        public long UnitCost = DefaultUnitCost;
    }

    public class Model
    {
        long _mana = 0;
        int _nextObjectId = 0;
        Config _config;
        long _time;

        public event Action<Fix64, Fix64, Fix64> OnInstantiate;
        public event Action OnDurationEnd;

        public Dictionary<byte,Attr> Results{ get; private set; }

        public float ManaView
        {
            get
            {
                return ((float)_mana) / (float)_config.MaxMana;
            }
        }

        bool Finished
        {
            get
            {
                return _time > _config.Duration;
            }
        }

        public string TimeString
        {
            get
            {
                var t = (_config.Duration - _time) / 1000;
                return string.Format("{0:D2}:{1:D2}", t/60, t%60);
            }
        }

        public Model(Config config)
        {
            _config = config;
            Results = new Dictionary<byte, Attr>();
        }

        public bool OnClick(Fix64 x, Fix64 y, Fix64 z, byte playerNum)
        {
            if (_mana < _config.UnitCost)
            {
                throw new Exception("Not enough mana");
            }
            if (OnInstantiate != null)
            {
                OnInstantiate(x, y, z);
            }
            _nextObjectId++;
            _mana -= _config.UnitCost;

            int clicks = 0;
            Attr result;
            if(Results.TryGetValue(playerNum, out result))
            {
                clicks = result.AsValue.ToInt();
            }
            Results[playerNum] = new AttrInt(clicks + 1);
            return true;
        }

        public void Simulate(long dt)
        {
            if(Finished)
            {
                return;
            }
            _time += dt;
            if(Finished)
            {
                if(OnDurationEnd != null)
                {
                    OnDurationEnd();
                }
                return;
            }
            _mana += dt * _config.ManaSpeed;
            if (_mana > _config.MaxMana)
            {
                _mana = _config.MaxMana;
            }
        }

        public void Reset()
        {
            _mana = 0;
            _time = 0;
            Results.Clear();
        }

    }
}