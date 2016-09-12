using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.GUIAnimation
{
    public static class CustomEasingUtility
    {
        public static void Invert(List<EasePoint> easeCustom)
        {
            for(int i = 0; i < easeCustom.Count; ++i)
            {
                EasePoint aEase = easeCustom[i];

                aEase.x = 1f - aEase.x;
                aEase.y = 1f - aEase.y;

                easeCustom[i] = aEase;
            }

            easeCustom.Sort(SortFuncton);
        }

        static int SortFuncton(EasePoint a, EasePoint b)
        {
            return a.x < b.x ? -1 : 1;
        }
    }
}
