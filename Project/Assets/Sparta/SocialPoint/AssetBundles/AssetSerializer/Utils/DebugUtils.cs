using System;
using UnityEngine;

namespace SocialPoint.AssetSerializer.Utils
{
    public sealed class DebugUtils
    {
        public static bool Verbose = false;

        public static void Log(string data)
        {
            if(Verbose)
            {
                Debug.Log(data);
            }
        }
    }
}

