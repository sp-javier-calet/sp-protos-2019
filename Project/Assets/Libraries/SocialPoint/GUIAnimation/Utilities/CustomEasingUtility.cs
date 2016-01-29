using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
    public static class CustomEasingUtility
    {
        public static void Invert (List<Vector2> easeCustom)
        {
            for (int i = 0; i < easeCustom.Count; ++i)
            {
                Vector2 aEase = easeCustom [i];

                aEase.x = 1f - aEase.x;
                aEase.y = 1f - aEase.y;

                easeCustom [i] = aEase;
            }

            easeCustom.Sort (SortFuncton);
        }

        static int SortFuncton (Vector2 a, Vector2 b)
        {
            return a.x < b.x ? -1 : 1;
        }
    }
}
