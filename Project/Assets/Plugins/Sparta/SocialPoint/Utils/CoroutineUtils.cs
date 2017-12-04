using UnityEngine;
using System.Collections;

namespace SocialPoint.Utils
{
    public static class CoroutineUtils
    {
        public static IEnumerator WaitForRealSeconds(float time)
        {
            float start = Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup < start + time)
            {
                yield return null;
            }
        }
    }
}
