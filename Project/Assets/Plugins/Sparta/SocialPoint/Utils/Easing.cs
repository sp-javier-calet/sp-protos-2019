using System.Collections.Generic;
using SocialPoint.Base;
using System;

namespace SocialPoint.Utils
{
    // Class that have all the predefined easing types

    public enum EaseType
    {
        Linear,

        InSine,
        OutSine,
        InOutSine,

        InQuad,
        OutQuad,
        InOutQuad,

        InCubic,
        OutCubic,
        InOutCubic,

        InQuart,
        OutQuart,
        InOutQuart,

        InQuint,
        OutQuint,
        InOutQuint,

        InExpo,
        OutExpo,
        InOutExpo,

        InCirc,
        OutCirc,
        InOutCirc,

        InElastic,
        OutElastic,
        InOutElastic,

        InBack,
        OutBack,
        InOutBack,

        InBounce,
        OutBounce,
        InOutBounce,

        OutDanielBounce,
    }

    [System.Serializable]
    public struct EasePoint
    {
        public float x;
        public float y;

        public EasePoint(float px, float py)
        {
            x = px;
            y = py;
        }
    }

    public static class Easing
    {
        public delegate float Function(float t, float b, float c, float d);

        public static Function ToFunction(this EaseType type)
        {
            switch(type)
            {
            case EaseType.Linear:
                return Linear;
            case EaseType.InSine:
                return InSine;
            case EaseType.OutSine:
                return OutSine;
            case EaseType.InOutSine:
                return InOutSine;
            case EaseType.InQuad:
                return InQuad;
            case EaseType.OutQuad:
                return OutQuad;
            case EaseType.InOutQuad:
                return InOutQuad;
            case EaseType.InCubic:
                return InCubic;
            case EaseType.OutCubic:
                return OutCubic;
            case EaseType.InOutCubic:
                return InOutCubic;
            case EaseType.InQuart:
                return InQuart;
            case EaseType.OutQuart:
                return OutQuart;
            case EaseType.InOutQuart:
                return InOutQuart;
            case EaseType.InQuint:
                return InQuint;
            case EaseType.OutQuint:
                return OutQuint;
            case EaseType.InOutQuint:
                return InOutQuint;
            case EaseType.InExpo:
                return InExpo;
            case EaseType.OutExpo:
                return OutExpo;
            case EaseType.InOutExpo:
                return InOutExpo;
            case EaseType.InCirc:
                return InCirc;
            case EaseType.OutCirc:
                return OutCirc;
            case EaseType.InOutCirc:
                return InOutCirc;
            case EaseType.InElastic:
                return InElastic;
            case EaseType.OutElastic:
                return OutElastic;
            case EaseType.InOutElastic:
                return InOutElastic;
            case EaseType.InBack:
                return InBack;
            case EaseType.OutBack:
                return OutBack;
            case EaseType.InOutBack:
                return InOutBack;
            case EaseType.InBounce:
                return InBounce;
            case EaseType.OutBounce:
                return OutBounce;
            case EaseType.InOutBounce:
                return InOutBounce;
            case EaseType.OutDanielBounce:
                return OutDanielBounce;
            }
            return null;
        }

        static float Sin(float v)
        {
            return (float)Math.Sin((double)v);
        }

        static float Cos(float v)
        {
            return (float)Math.Cos((double)v);
        }

        static float Pow(float b, float e)
        {
            return (float)Math.Pow((double)b, (double)e);
        }

        static float Sqrt(float v)
        {
            return (float)Math.Sqrt((double)v);
        }

        static float Asin(float v)
        {
            return (float)Math.Asin((double)v);
        }

        static int Min(int a, int b)
        {
            return Math.Min(a, b);
        }

        static float Abs(float v)
        {
            return (float)Math.Abs((double)v);
        }

        static float _epsilon = 1e-4f;

        static bool IsEqual(float a, float b)
        {
            return Abs(a - b) <= _epsilon;
        }

        static float Lerp(float a, float b, float t)
        {
            return a * (1f - t) + b * t;
        }

        static bool IsZero(float v)
        {
            return Equals(v, 0f);
        }

        static float PI = (float)Math.PI;

        public static float Linear(float t, float b, float c, float d)
        {
            return Lerp(b, b + c, t / d);
        }

        public static float InSine(float t, float b, float c, float d)
        {
            return -c * Cos(t / d * (PI / 2f)) + c + b;
        }
            
        public static float OutSine(float t, float b, float c, float d)
        {
            return c * Sin(t / d * (PI / 2f)) + b;
        }

        public static float InOutSine(float t, float b, float c, float d)
        {
            return -c / 2f * (Cos(PI * t / d) - 1f) + b;
        }

        public static float InQuad(float t, float b, float c, float d)
        {
            return c * (t /= d) * t + b;
        }

        public static float OutQuad(float t, float b, float c, float d)
        {
            return -c * (t /= d) * (t - 2f) + b;
        }

        public static float InOutQuad(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
                return c / 2f * t * t + b;
            return -c / 2f * ((--t) * (t - 2f) - 1f) + b;
        }

        public static float InCubic(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t + b;
        }

        public static float OutCubic(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1f) * t * t + 1f) + b;
        }

        public static float InOutCubic(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
                return c / 2f * t * t * t + b;
            return c / 2f * ((t -= 2f) * t * t + 2f) + b;
        }

        public static float InQuart(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t * t + b;
        }

        public static float OutQuart(float t, float b, float c, float d)
        {
            return -c * ((t = t / d - 1f) * t * t * t - 1f) + b;
        }

        public static float InOutQuart(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
                return c / 2f * t * t * t * t + b;
            return -c / 2f * ((t -= 2f) * t * t * t - 2f) + b;
        }

        public static float InQuint(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t * t * t + b;
        }

        public static float OutQuint(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1f) * t * t * t * t + 1f) + b;
        }

        public static float InOutQuint(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
                return c / 2f * t * t * t * t * t + b;
            return c / 2f * ((t -= 2f) * t * t * t * t + 2f) + b;
        }

        public static float InExpo(float t, float b, float c, float d)
        {
            return (t == 0f) ? b : c * Pow(2f, 10f * (t / d - 1f)) + b;
        }

        public static float OutExpo(float t, float b, float c, float d)
        {
            return (t == d) ? b + c : c * (-Pow(2f, -10f * t / d) + 1f) + b;
        }

        public static float InOutExpo(float t, float b, float c, float d)
        {
            if(t == 0f)
                return b;
            if(t == d)
                return b + c;
            if((t /= d / 2f) < 1f)
                return c / 2f * Pow(2f, 10f * (t - 1f)) + b;
            return c / 2 * (-Pow(2f, -10f * --t) + 2f) + b;
        }

        public static float InCirc(float t, float b, float c, float d)
        {
            return -c * (Sqrt(1f - (t /= d) * t) - 1f) + b;
        }

        public static float OutCirc(float t, float b, float c, float d)
        {
            return c * Sqrt(1f - (t = t / d - 1f) * t) + b;
        }

        public static float InOutCirc(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
                return -c / 2f * (Sqrt(1f - t * t) - 1f) + b;
            return c / 2 * (Sqrt(1 - (t -= 2) * t) + 1) + b;
        }

        public static float InElastic(float t, float b, float c, float d)
        {
            float s = 1.70158f;
            float p = 0f;
            float a = c;
            if(IsEqual(t, 0f))
                return b;
            if(IsEqual((t /= d), 1f))
                return b + c;
            if(!IsZero(p))
                p = d * 0.3f;
            if(a < Abs(c))
            {
                a = c;
                s = p / 4f;
            }
            else
                s = p / (2f * PI) * Asin(c / a);
            return -(a * Pow(2f, 10f * (t -= 1f)) * Sin((t * d - s) * (2f * PI) / p)) + b;
        }

        public static float OutElastic(float t, float b, float c, float d)
        {
            if(t == 0d)
                return b;
            if((t /= d) == 1f)
                return b + c;  
            float p = d * 0.3f;
            float a = c; 
            float s = p / 4f;
            return (a * Pow(2f, -10f * t) * Sin((t * d - s) * (2f * PI) / p) + c + b);   
        }

        public static float InOutElastic(float t, float b, float c, float d)
        {
            if(t == 0f)
                return b;
            if((t /= d / 2f) == 2f)
                return b + c; 
            float p = d * (0.3f * 1.5f);
            float a = c; 
            float s = p / 4f;

            if(t < 1f)
            {
                float postFix = a * Pow(2f, 10f * (t -= 1f)); // postIncrement is evil
                return -0.5f * (postFix * Sin((t * d - s) * (2f * PI) / p)) + b;
            } 
            float postFix2 = a * Pow(2f, -10f * (t -= 1f)); // postIncrement is evil
            return postFix2 * Sin((t * d - s) * (2f * PI) / p) * 0.5f + c + b;
        }

        public static float InBack(float t, float b, float c, float d)
        {
            float s = 1.70158f;
            return c * (t /= d) * t * ((s + 1f) * t - s) + b;
        }

        public static float OutBack(float t, float b, float c, float d)
        {
            float s = 1.70158f;
            return c * ((t = t / d - 1f) * t * ((s + 1f) * t + s) + 1f) + b;
        }

        public static float InOutBack(float t, float b, float c, float d)
        {
            float s = 1.70158f; 
            if((t /= d / 2f) < 1f)
                return c / 2f * (t * t * (((s *= (1.525f)) + 1f) * t - s)) + b;
            return c / 2f * ((t -= 2f) * t * (((s *= (1.525f)) + 1f) * t + s) + 2f) + b;
        }

        public static float InBounce(float t, float b, float c, float d)
        {
            return c - OutBounce(d - t, 0f, c, d) + b;
        }

        public static float OutBounce(float t, float b, float c, float d)
        {
            if((t /= d) < (1f / 2.75f))
            {
                return c * (7.5625f * t * t) + b;
            }
            else if(t < (2f / 2.75f))
            {
                return c * (7.5625f * (t -= (1.5f / 2.75f)) * t + 0.75f) + b;
            }
            else if(t < (2.5f / 2.75f))
            {
                return c * (7.5625f * (t -= (2.25f / 2.75f)) * t + 0.9375f) + b;
            }
            else
            {
                return c * (7.5625f * (t -= (2.625f / 2.75f)) * t + 0.984375f) + b;
            }
        }

        public static float InOutBounce(float t, float b, float c, float d)
        {
            if(t < d / 2f)
                return InBounce(t * 2f, 0f, c, d) * 0.5f + b;
            return OutBounce(t * 2f - d, 0f, c, d) * 0.5f + c * 0.5f + b;
        }
            
        public static float OutDanielBounce(float t, float b, float c, float d)
        {
            float s = 5f;
            return c * ((t = t / d - 1f) * t * ((s + 1f) * t + s) + 1f) + b;
        }

        public static float Custom(float t, float d, List<EasePoint> timeValues)
        {
            if(timeValues.Count == 0)
            {
                Log.w("TimeValues is Empty :(");
                return 0f;
            }

            float tnorm = t / d;

            int startIdx = 0;
            int endIdx = 0;

            for(int i = timeValues.Count - 1; i >= 0; i--)
            {
                if(timeValues[i].x <= tnorm)
                {
                    startIdx = i;
                    break;
                }
            }

            endIdx = Min(startIdx + 1, timeValues.Count - 1);

            var start = timeValues[startIdx];
            var end = timeValues[endIdx];

            float deltaT = (end.x - start.x);
            float lerpValue = 0f;

            if(Abs(deltaT) < 1e-5f)
            {
                lerpValue = end.y;
            }
            else
            {
                float localT = (tnorm - start.x) / (end.x - start.x);
                lerpValue = start.y + (end.y - start.y) * localT;
            }

            return lerpValue;
        }

        public static void InvertCustom(List<EasePoint> easeCustom)
        {
            for (int i = 0; i < easeCustom.Count; ++i)
            {
                var aEase = easeCustom [i];

                aEase.x = 1f - aEase.x;
                aEase.y = 1f - aEase.y;

                easeCustom [i] = aEase;
            }

            easeCustom.Sort (SortFuncton);
        }

        static int SortFuncton (EasePoint a, EasePoint b)
        {
            return a.x < b.x ? -1 : 1;
        }
    }
}
