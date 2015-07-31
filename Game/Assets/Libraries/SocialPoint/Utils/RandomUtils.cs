using System;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
    public class RandomUtils
    {
        public static string GetUuid(string format=null)
        {
            Guid g = System.Guid.NewGuid();
            return g.ToString(format);
        }

        public static UInt64 GenerateUserId()
        {
            UInt64 ts = (UInt64)TimeUtils.Timestamp;
            UInt64 rn = (UInt64)GenerateRandom32();
            ts = ts << 31;
            rn = rn & 0x7FFFFFFF;
            return ts + rn;
        }

        private static uint GenerateRandom32()
        {
            return (uint)UnityEngine.Random.Range(-int.MaxValue, int.MaxValue);
        }

        [System.Obsolete("Use GenerateSecurityToken instead", true)]
        public static string GenerateClientToken()
        {
            return GenerateSecurityToken();
        }

        public static string GenerateSecurityToken()
        {
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
    }
}
