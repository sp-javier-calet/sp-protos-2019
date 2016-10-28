using System;
using FixMath.NET;

namespace SocialPoint.Utils
{
    public sealed class XRandom
    {
        public static uint GenerateSeed()
        {
            return (uint)(new Random().Next());
        }

        readonly LinearCongruentialEngine _lce;

        public XRandom(uint seed)
        {
            _lce = new LinearCongruentialEngine(seed);
        }

        public uint Next()
        {
            return Range(LinearCongruentialEngine.Min, LinearCongruentialEngine.Max);
        }

        public uint Range(uint min, uint max)
        {
            return _lce.Next % (max - min) + min;
        }

        public int Range(int min, int max)
        {
            return (int)(_lce.Next % (max - min) + min);
        }

        public Fix64 Range(Fix64 min, Fix64 max)
        {         
            return Fix64.FromRaw(_lce.Next64 % (max.RawValue - min.RawValue) + min.RawValue);
        }

        #region Linear concruential engine

        class LinearCongruentialEngine
        {
            const uint _a = 48271;
            const uint _c = 0;
            const uint _m = 2147483647;

            uint _previous;

            public LinearCongruentialEngine(uint seed)
            {
                _previous = seed == 0 ? 1 : seed;
            }

            public uint Next
            {
                get
                {
                    _previous = (uint)((_a * (UInt64)_previous + _c)  % _m);
                    return _previous;
                }
            }

            public long Next64
            {
                get
                {
                    long r64 = 0;
                    r64 = Next;
                    r64 = r64 << 32;
                    r64 += Next;
                    return r64;
                }
            }

            public static uint Min 
            {
                get
                {
                    return 0;
                }
            }

            public static uint Max
            {
                get
                {
                    return _m - 1;
                }
            }
        }

        #endregion
    }
}
