#if UNITY_EDITOR_OSX
#define NATIVE_RANDOM
#elif UNITY_ANDROID
#define NATIVE_RANDOM
#elif UNITY_IOS
#define NATIVE_RANDOM
#endif

using System;
using System.Runtime.InteropServices;
using SocialPoint.Base;
using UnityEngine;
using UnityEngine.Assertions;

namespace SocialPoint.Utils
{
    public static class RandomUtils
    {
        static bool _init = false;


        #if UNITY_EDITOR_OSX
        const string PluginModuleName = "SPUnityPlugins";
        #elif UNITY_ANDROID
        const string PluginModuleName = "sp_unity_utils";
        #elif UNITY_IOS
        const string PluginModuleName = "__Internal";
        #endif

        #if NATIVE_RANDOM

        static int SPUnityUtilsGetRandomSeed()
        {
            return SPUnityUtilsGetRandomInt();
        }

        [DllImport(PluginModuleName)]
        static extern int SPUnityUtilsGetRandomInt();

        [DllImport(PluginModuleName)]
        static extern uint SPUnityUtilsGetRandomUnsignedInt();

        [DllImport(PluginModuleName)]
        static extern int SPUnityUtilsGetRandomIntRange(int min, int max);

        [DllImport(PluginModuleName)]
        static extern float SPUnityUtilsGetRandomFloatRange(float min, float max);

        #else

        static int SPUnityUtilsGetRandomSeed()
        {
            Debug.LogWarning("Using substandard Random implementation, this should only happen in Editor!");
            return TimeUtils.Timestamp.GetHashCode() ^ Guid.NewGuid().GetHashCode();
        }

        static int SPUnityUtilsGetRandomInt()
        {
            return UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        static uint SPUnityUtilsGetRandomUnsignedInt()
        {
            return (uint)UnityEngine.Random.Range(-int.MaxValue, int.MaxValue);
        }

        static int SPUnityUtilsGetRandomIntRange(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        static float SPUnityUtilsGetRandomFloatRange(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        #endif

        static void Init()
        {
            if(_init)
            {
                return;
            }
            UnityEngine.Random.seed = SPUnityUtilsGetRandomSeed();
            _init = true;
        }

        public static string GetUuid(string format = null)
        {
            Init();
            var g = Guid.NewGuid();
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
            return SPUnityUtilsGetRandomUnsignedInt();
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
            Assert.IsTrue(minInclusive < maxExclusive, "Max needs to be more that min.");
            return SPUnityUtilsGetRandomIntRange(minInclusive, maxExclusive-1);
        }

        /// <summary>
        /// Return a random float value between the param range [min, max]
        /// </summary>
        /// <param name="minInclusive">Minimum inclusive.</param>
        /// <param name="maxExclusive">Max inclusive.</param>
        public static float Range(float minInclusive, float maxInclusive)
        {
            Init();
            Assert.IsTrue(minInclusive < maxInclusive, "Max needs to be more that min.");
            return SPUnityUtilsGetRandomFloatRange(minInclusive, maxInclusive);
        }
    }
}
