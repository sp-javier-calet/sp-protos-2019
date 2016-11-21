using System;
using FixMath.NET;
using SocialPoint.Attributes;
using System.Collections.Generic;

namespace Examples.Lockstep
{
    public class Model
    {
        long _mana = 0;
        int _nextObjectId = 0;

        const long ManaSpeed = 2;
        const long MaxMana = 10000;

        const long UnitCost = 7000;

        public event Action<Fix64, Fix64, Fix64> OnInstantiate;

        public Dictionary<byte,Attr> Results{ get; private set; }

        public float ManaView
        {
            get
            {
                return ((float)_mana) / (float)MaxMana;
            }
        }

        public Model()
        {
            Results = new Dictionary<byte, Attr>();
        }

        public bool OnClick(Fix64 x, Fix64 y, Fix64 z, byte playerNum)
        {
            if (_mana < UnitCost)
            {
                throw new Exception("Not enough mana");
            }
            if (OnInstantiate != null)
            {
                OnInstantiate(x, y, z);
            }
            _nextObjectId++;
            _mana -= UnitCost;

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
            _mana += dt * ManaSpeed;
            if (_mana > MaxMana)
            {
                _mana = MaxMana;
            }
        }

        public void Reset()
        {
            _mana = 0;
            Results.Clear();
        }

    }
}