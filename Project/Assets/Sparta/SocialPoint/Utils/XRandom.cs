using System;

namespace SocialPoint.Utils
{

    public sealed class XRandom
    {
        public static uint GenerateSeed()
        {
            return (uint)(new Random().Next());
        }

        LinearCongruentialEngine _lce;

        double Value
        {
            get
            {
                return (double) _lce.Next / (double)LinearCongruentialEngine.Max;
            }
        }

        public XRandom(uint seed)
        {
            _lce = new LinearCongruentialEngine(seed);
        }

        public int Range(int min, int max)
        {
            return (int)(Value * (max - min + 1) % (max - min + 1) + min);
        }

        public float Range(float min, float max)
        {
            return (float)(Value * (max - min) + min);
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
