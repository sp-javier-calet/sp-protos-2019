using System;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Utils
{
    public static class RandomUtils
    {
        static bool _init = false;

        static void Init()
        {
            if(_init)
            {
                return;
            }
            var devId = SystemInfo.deviceUniqueIdentifier.GetHashCode();
            var guId = System.Guid.NewGuid().GetHashCode();
            UnityEngine.Random.seed ^= devId;
            UnityEngine.Random.seed ^= guId;
            _init = true;
        }

        public static string GetUuid(string format = null)
        {
            Init();
            Guid g = System.Guid.NewGuid();
            return g.ToString(format);
        }

        public static UInt64 GenerateUserId()
        {
            Init();
            UInt64 ts = (UInt64)TimeUtils.Timestamp;
            UInt64 rn = (UInt64)GenerateRandom32();
            ts = ts << 31;
            rn = rn & 0x7FFFFFFF;
            return ts + rn;
        }

        private static uint GenerateRandom32()
        {
            Init();
            return (uint)UnityEngine.Random.Range(-int.MaxValue, int.MaxValue);
        }

        [System.Obsolete("Use GenerateSecurityToken instead", true)]
        public static string GenerateClientToken()
        {
            Init();
            return GenerateSecurityToken();
        }

        public static string GenerateSecurityToken()
        {
            Init();
            uint ab1 = GenerateRandom32();
            uint ab2 = GenerateRandom32();
            ulong ab = (ulong)(ab1 << 32 | ab2);

            uint cd1 = GenerateRandom32();
            uint cd2 = GenerateRandom32();
            ulong cd = (ulong)(cd1 << 32 | cd2);

            ab = (ab & 0xFFFFFFFFFFFF0FFFULL) | 0x0000000000004000ULL;
            cd = (cd & 0x3FFFFFFFFFFFFFFFULL) | 0x8000000000000000ULL;

            //ulong a = (ulong) (ab >> 32);
            ulong b = (ulong)(ab & 0xFFFFFFFF);
            ulong c = (ulong)(cd >> 32);
            ulong d = (ulong)(cd & 0xFFFFFFFF);

            string s = "";

            s += ab1.ToString("X").PadLeft(8, '0') + "-";
            s += (b >> 16).ToString("X").PadLeft(4, '0') + "-";
            s += (b & 0xFFFF).ToString("X").PadLeft(4, '0') + "-";
            s += (c >> 16).ToString("X").PadLeft(4, '0') + "-";
            s += (c & 0xFFFF).ToString("X").PadLeft(4, '0');
            s += (d).ToString("X").PadLeft(8, '0');

            return s.ToLower();
        }

        /// <summary>
        /// Return a random int value between the param range [min, max)
        /// </summary>
        /// <param name="minInclusive">Minimum inclusive.</param>
        /// <param name="maxExclusive">Max exclusive.</param>
        public static int Range(int minInclusive, int maxExclusive)
        {
            Init();
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }

        /// <summary>
        /// Return a random float value between the param range [min, max]
        /// </summary>
        /// <param name="minInclusive">Minimum inclusive.</param>
        /// <param name="maxExclusive">Max inclusive.</param>
        public static float Range(float minInclusive, float maxInclusive)
        {
            Init();
            return UnityEngine.Random.Range(minInclusive, maxInclusive);
        }
    }
}
