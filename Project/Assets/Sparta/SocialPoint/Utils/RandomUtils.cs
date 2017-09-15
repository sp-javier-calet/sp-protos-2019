#if (UNITY_ANDROID || UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
#define UNITY_DEVICE
#endif
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_DEVICE
#define NATIVE_RANDOM
#define UNITY
#endif

using System;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
    public static class RandomUtils
    {
        static bool _init = false;

        #if !UNITY
        static Random _random;
        #endif


        #if NATIVE_RANDOM

        #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        const string PluginModuleName = "SPUnityPlugins";
        #elif UNITY_ANDROID
        const string PluginModuleName = "sp_unity_utils";
#elif (UNITY_IOS || UNITY_TVOS)
        const string PluginModuleName = "__Internal";
        #endif

        [System.Runtime.InteropServices.DllImport(PluginModuleName)]
        static extern int SPUnityUtilsGetRandomInt();

        static int GetRandomSeed()
        {
            return SPUnityUtilsGetRandomInt();
        }

        #else
        
        static int GetRandomSeed()
        {
            Log.w("Using substandard Random implementation, this should only happen in Windows!");
            return TimeUtils.Timestamp.GetHashCode() ^ Guid.NewGuid().GetHashCode();
        }

        #endif

        static void Init()
        {
            if(_init)
            {
                return;
            }
            #if UNITY
            InitRandomSeed();
            #else
            _random = new Random(Guid.NewGuid().GetHashCode());
            #endif
            _init = true;
        }

    #if UNITY
        static void InitRandomSeed()
        {
            UnityEngine.Random.InitState(GetRandomSeed());
        }
    #endif

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
            UInt64 rn = (UInt64)GenerateUint();
            ts = ts << 31;
            rn = rn & 0x7FFFFFFF;
            return ts + rn;
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
            uint ab1 = GenerateUint();
            uint ab2 = GenerateUint();
            ulong ab = (ulong)(ab1 << 32 | ab2);

            uint cd1 = GenerateUint();
            uint cd2 = GenerateUint();
            ulong cd = (ulong)(cd1 << 32 | cd2);

            ab = (ab & 0xFFFFFFFFFFFF0FFFUL) | 0x0000000000004000UL;
            cd = (cd & 0x3FFFFFFFFFFFFFFFUL) | 0x8000000000000000UL;

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

        public static uint GenerateUint()
        {
            Init();
            #if UNITY
            return (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            #else
            return (uint)_random.Next(Int32.MinValue, Int32.MaxValue);
            #endif
        }

        /// <summary>
        /// Return a random int value between the param range [min, max)
        /// </summary>
        /// <param name="minInclusive">Minimum inclusive.</param>
        /// <param name="maxExclusive">Max exclusive.</param>
        public static int Range(int minInclusive, int maxExclusive)
        {
            Init();
            DebugUtils.Assert(minInclusive < maxExclusive, "Max needs to be more that min.");
            #if UNITY
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
            #else
            return _random.Next(minInclusive, maxExclusive);
            #endif

        }

        /// <summary>
        /// Return a random float value between the param range [min, max]
        /// </summary>
        /// <param name="minInclusive">Minimum inclusive.</param>
        /// <param name="maxInclusive">Max inclusive.</param>
        public static float Range(float minInclusive, float maxInclusive)
        {
            Init();
            DebugUtils.Assert(minInclusive < maxInclusive, "Max needs to be more that min.");
            #if UNITY
            return UnityEngine.Random.Range(minInclusive, maxInclusive);
            #else
            return (float)_random.NextDouble() * (maxInclusive - minInclusive) + minInclusive;
            #endif
        }
    }
}
    
