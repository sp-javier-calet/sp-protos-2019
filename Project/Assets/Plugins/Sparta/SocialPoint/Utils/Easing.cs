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

    [Serializable]
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

    /// <summary>
    /// A set of easing methods, to see a visual representation you can check out:
    /// https://msdn.microsoft.com/en-us/library/vstudio/Ee308751%28v=VS.100%29.aspx
    /// </summary>
    public static class Easing
    {
        const float kEPSILON = 1e-4f;
        const float kPI = (float)Math.PI;
        const float kHALF_PI = (float)(Math.PI * 0.5f);
        const float kDOUBLE_PI = (float)(Math.PI * 2f);

        public delegate float Function(float t, float b, float c, float d);

        public static Function ToFunction(this EaseType type)
        {
            switch(type)
            {
                case EaseType.Linear:
                    return LinearEase;
                case EaseType.InSine:
                    return SineEaseIn;
                case EaseType.OutSine:
                    return SineEaseOut;
                case EaseType.InOutSine:
                    return SineEaseInOut;
                case EaseType.InQuad:
                    return QuadEaseIn;
                case EaseType.OutQuad:
                    return QuadEaseOut;
                case EaseType.InOutQuad:
                    return QuadEaseInOut;
                case EaseType.InCubic:
                    return CubicEaseIn;
                case EaseType.OutCubic:
                    return CubicEaseOut;
                case EaseType.InOutCubic:
                    return CubicEaseInOut;
                case EaseType.InQuart:
                    return QuartEaseIn;
                case EaseType.OutQuart:
                    return QuartEaseOut;
                case EaseType.InOutQuart:
                    return QuartEaseInOut;
                case EaseType.InQuint:
                    return QuintEaseIn;
                case EaseType.OutQuint:
                    return QuintEaseOut;
                case EaseType.InOutQuint:
                    return QuintEaseInOut;
                case EaseType.InExpo:
                    return ExpoEaseIn;
                case EaseType.OutExpo:
                    return ExpoEaseOut;
                case EaseType.InOutExpo:
                    return ExpoEaseInOut;
                case EaseType.InCirc:
                    return CircleEaseIn;
                case EaseType.OutCirc:
                    return CircleEaseOut;
                case EaseType.InOutCirc:
                    return CircleEaseInOut;
                case EaseType.InElastic:
                    return ElasticEaseIn;
                case EaseType.OutElastic:
                    return ElasticEaseOut;
                case EaseType.InOutElastic:
                    return ElasticEaseInOut;
                case EaseType.InBack:
                    return BackEaseIn;
                case EaseType.OutBack:
                    return BackEaseOut;
                case EaseType.InOutBack:
                    return BackEaseInOut;
                case EaseType.InBounce:
                    return BounceEaseIn;
                case EaseType.OutBounce:
                    return BounceEaseOut;
                case EaseType.InOutBounce:
                    return BounceEaseInOut;
                case EaseType.OutDanielBounce:
                    return OutDanielBounce;
            }
            return null;
        }

        static float Sin(float v)
        {
            return (float)Math.Sin(v);
        }

        static float Cos(float v)
        {
            return (float)Math.Cos(v);
        }

        static float Pow(float b, float e)
        {
            return (float)Math.Pow(b, e);
        }

        static float Sqrt(float v)
        {
            return (float)Math.Sqrt(v);
        }

        static float Asin(float v)
        {
            return (float)Math.Asin(v);
        }

        static int Min(int a, int b)
        {
            return Math.Min(a, b);
        }

        static float Abs(float v)
        {
            return (float)Math.Abs((double)v);
        }

        static bool IsEqual(float a, float b)
        {
            return Abs(a - b) <= kEPSILON;
        }

        static float Lerp(float a, float b, float t)
        {
            return a * (1f - t) + b * t;
        }

        static bool IsZero(float v)
        {
            return Equals(v, 0f);
        }

        #region obsolete methods

        [Obsolete("Use LinearEase method")]
        public static float Linear(float t, float b, float c, float d)
        {
            return LinearEase(t, b, c, d);
        }

        [Obsolete("Use SineEaseIn method")]
        public static float InSine(float t, float b, float c, float d)
        {
            return SineEaseIn(t, b, c, d);
        }

        [Obsolete("Use SineEaseOut method")]
        public static float OutSine(float t, float b, float c, float d)
        {
            return SineEaseOut(t, b, c, d);
        }

        [Obsolete("Use SineEaseInOut method")]
        public static float InOutSine(float t, float b, float c, float d)
        {
            return SineEaseInOut(t, b, c, d);
        }

        [Obsolete("Use SineEaseInOut method")]
        public static float InQuad(float t, float b, float c, float d)
        {
            return QuadEaseIn(t, b, c, d);
        }

        [Obsolete("Use QuadEaseOut method")]
        public static float OutQuad(float t, float b, float c, float d)
        {
            return QuadEaseOut(t, b, c, d);
        }

        [Obsolete("Use QuadEaseInOut method")]
        public static float InOutQuad(float t, float b, float c, float d)
        {
            return QuadEaseInOut(t, b, c, d);
        }

        [Obsolete("Use CubicEaseIn method")]
        public static float InCubic(float t, float b, float c, float d)
        {
            return CubicEaseIn(t, b, c, d);
        }

        [Obsolete("Use CubicEaseOut method")]
        public static float OutCubic(float t, float b, float c, float d)
        {
            return CubicEaseOut(t, b, c, d);
        }

        [Obsolete("Use CubicEaseInOut method")]
        public static float InOutCubic(float t, float b, float c, float d)
        {
            return CubicEaseInOut(t, b, c, d);
        }

        [Obsolete("Use QuartEaseIn method")]
        public static float InQuart(float t, float b, float c, float d)
        {
            return QuartEaseIn(t, b, c, d);
        }

        [Obsolete("Use QuartEaseOut method")]
        public static float OutQuart(float t, float b, float c, float d)
        {
            return QuartEaseOut(t, b, c, d);
        }

        [Obsolete("Use QuartEaseInOut method")]
        public static float InOutQuart(float t, float b, float c, float d)
        {
            return QuartEaseInOut(t, b, c, d);
        }

        [Obsolete("Use QuintEaseIn method")]
        public static float InQuint(float t, float b, float c, float d)
        {
            return QuintEaseIn(t, b, c, d);
        }

        [Obsolete("Use QuintEaseOut method")]
        public static float OutQuint(float t, float b, float c, float d)
        {
            return QuintEaseOut(t, b, c, d);
        }

        [Obsolete("Use QuintEaseInOut method")]
        public static float InOutQuint(float t, float b, float c, float d)
        {
            return QuintEaseInOut(t, b, c, d);
        }

        [Obsolete("Use ExpoEaseIn method")]
        public static float InExpo(float t, float b, float c, float d)
        {
            return ExpoEaseIn(t, b, c, d);
        }

        [Obsolete("Use ExpoEaseOut method")]
        public static float OutExpo(float t, float b, float c, float d)
        {
            return ExpoEaseOut(t, b, c, d);
        }

        [Obsolete("Use ExpoEaseInOut method")]
        public static float InOutExpo(float t, float b, float c, float d)
        {
            return ExpoEaseInOut(t, b, c, d);
        }

        [Obsolete("Use CircleEaseIn method")]
        public static float InCirc(float t, float b, float c, float d)
        {
            return CircleEaseIn(t, b, c, d);
        }

        [Obsolete("Use CircleEaseOut method")]
        public static float OutCirc(float t, float b, float c, float d)
        {
            return CircleEaseOut(t, b, c, d);
        }

        [Obsolete("Use CircleEaseInOut method")]
        public static float InOutCirc(float t, float b, float c, float d)
        {
            return CircleEaseInOut(t, b, c, d);
        }

        [Obsolete("Use ElasticEaseIn method")]
        public static float InElastic(float t, float b, float c, float d)
        {
            return ElasticEaseIn(t, b, c, d);
        }

        [Obsolete("Use ElasticEaseOut method")]
        public static float OutElastic(float t, float b, float c, float d)
        {
            return ElasticEaseOut(t, b, c, d);
        }

        [Obsolete("Use ElasticEaseInOut method")]
        public static float InOutElastic(float t, float b, float c, float d)
        {
            return ElasticEaseInOut(t, b, c, d);
        }

        [Obsolete("Use BackEaseIn method")]
        public static float InBack(float t, float b, float c, float d)
        {
            return BackEaseIn(t, b, c, d);
        }

        [Obsolete("Use BackEaseOut method")]
        public static float OutBack(float t, float b, float c, float d)
        {
            return BackEaseOut(t, b, c, d);
        }

        [Obsolete("Use BackEaseInOut method")]
        public static float InOutBack(float t, float b, float c, float d)
        {
            return BackEaseInOut(t, b, c, d);
        }

        [Obsolete("Use BounceEaseIn method")]
        public static float InBounce(float t, float b, float c, float d)
        {
            return BounceEaseIn(t, b, c, d);
        }

        [Obsolete("Use BounceEaseOut method")]
        public static float OutBounce(float t, float b, float c, float d)
        {
            return BounceEaseOut(t, b, c, d);
        }

        [Obsolete("Use BounceEaseInOut method")]
        public static float InOutBounce(float t, float b, float c, float d)
        {
            return BounceEaseInOut(t, b, c, d);
        }

        #endregion

        // Creates an animation that uses a linear curve.
        #region LinearEase

        public static float LinearEase(float t, float b, float c, float d)
        {
            return Lerp(b, b + c, t / d);
        }

        #endregion

        // Creates an animation that accelerates and/or decelerates using a sine formula.
        // https://msdn.microsoft.com/en-us/library/system.windows.media.animation.sineease(v=vs.110).aspx
        #region SineEase

        public static float SineEaseIn(float t, float b, float c, float d)
        {
            return -c * Cos(t / d * kHALF_PI) + c + b;
        }

        public static float SineEaseOut(float t, float b, float c, float d)
        {
            return c * Sin(t / d * kHALF_PI) + b;
        }

        public static float SineEaseInOut(float t, float b, float c, float d)
        {
            return -c / 2f * (Cos(kPI * t / d) - 1f) + b;
        }

        #endregion

        // Creates an animation that accelerates and/or decelerates using the formula f(t) = t2.
        // https://msdn.microsoft.com/en-us/library/system.windows.media.animation.quarticease(v=vs.110).aspx
        #region QuadraticEase

        public static float QuadEaseIn(float t, float b, float c, float d)
        {
            return c * (t /= d) * t + b;
        }

        public static float QuadEaseOut(float t, float b, float c, float d)
        {
            return -c * (t /= d) * (t - 2f) + b;
        }

        public static float QuadEaseInOut(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
            {
                return c / 2f * t * t + b;
            }

            return -c / 2f * ((--t) * (t - 2f) - 1f) + b;
        }

        #endregion

        // Creates an animation that accelerates and/or decelerates using the formula f(t) = t3.
        // https://msdn.microsoft.com/en-us/library/system.windows.media.animation.cubicease(v=vs.110).aspx
        #region CubicEase

        public static float CubicEaseIn(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t + b;
        }

        public static float CubicEaseOut(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1f) * t * t + 1f) + b;
        }

        public static float CubicEaseInOut(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
            {
                return c / 2f * t * t * t + b;
            }

            return c / 2f * ((t -= 2f) * t * t + 2f) + b;
        }

        #endregion

        // Creates an animation that accelerates and/or decelerates using the formula f(t) = t4.
        // https://msdn.microsoft.com/en-us/library/system.windows.media.animation.quadraticease(v=vs.110).aspx
        #region QuarticEase

        public static float QuartEaseIn(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t * t + b;
        }

        public static float QuartEaseOut(float t, float b, float c, float d)
        {
            return -c * ((t = t / d - 1f) * t * t * t - 1f) + b;
        }

        public static float QuartEaseInOut(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
            {
                return c / 2f * t * t * t * t + b;
            }

            return -c / 2f * ((t -= 2f) * t * t * t - 2f) + b;
        }

        #endregion

        // Create an animation that accelerates and/or decelerates using the formula f(t) = t5.
        // https://msdn.microsoft.com/en-us/library/system.windows.media.animation.quinticease(v=vs.110).aspx
        #region QuinticEase

        public static float QuintEaseIn(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t * t * t + b;
        }

        public static float QuintEaseOut(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1f) * t * t * t * t + 1f) + b;
        }

        public static float QuintEaseInOut(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
            {
                return c / 2f * t * t * t * t * t + b;
            }

            return c / 2f * ((t -= 2f) * t * t * t * t + 2f) + b;
        }

        #endregion

        // Creates an animation that accelerates and/or decelerates using an exponential formula.
        // https://msdn.microsoft.com/en-us/library/system.windows.media.animation.exponentialease(v=vs.110).aspx
        #region ExponentialEase

        public static float ExpoEaseIn(float t, float b, float c, float d)
        {
            return IsEqual(t, 0f) ? b : c * Pow(2f, 10f * (t / d - 1f)) + b;
        }

        public static float ExpoEaseOut(float t, float b, float c, float d)
        {
            return IsEqual(t, d) ? b + c : c * (-Pow(2f, -10f * t / d) + 1f) + b;
        }

        public static float ExpoEaseInOut(float t, float b, float c, float d)
        {
            if(IsEqual(t, 0f))
            {
                return b;
            }

            if(IsEqual(t, d))
            {
                return b + c;
            }

            if((t /= d / 2f) < 1f)
            {
                return c / 2f * Pow(2f, 10f * (t - 1f)) + b;
            }

            return c / 2 * (-Pow(2f, -10f * --t) + 2f) + b;
        }

        #endregion

        // Creates an animation that accelerates and/or decelerates using a circular function.
        // https://msdn.microsoft.com/en-us/library/system.windows.media.animation.circleease(v=vs.110).aspx
        #region CircleEase

        public static float CircleEaseIn(float t, float b, float c, float d)
        {
            return -c * (Sqrt(1f - (t /= d) * t) - 1f) + b;
        }

        public static float CircleEaseOut(float t, float b, float c, float d)
        {
            return c * Sqrt(1f - (t = t / d - 1f) * t) + b;
        }

        public static float CircleEaseInOut(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
            {
                return -c / 2f * (Sqrt(1f - t * t) - 1f) + b;
            }

            return c / 2f * (Sqrt(1f - (t -= 2f) * t) + 1f) + b;
        }

        #endregion

        // Creates an animation that resembles a spring oscillating back and forth until it comes to rest.
        // https://msdn.microsoft.com/en-us/library/system.windows.media.animation.elasticease(v=vs.110).aspx
        #region ElasticEase

        public static float ElasticEaseIn(float t, float b, float c, float d)
        {
            var s = 1.70158f;
            var p = 0f;
            var a = c;

            if(IsEqual(t, 0f))
            {
                return b;
            }

            if(IsEqual((t /= d), 1f))
            {
                return b + c;
            }

            if(!IsZero(p))
            {
                p = d * 0.3f;
            }

            if(a < Abs(c))
            {
                a = c;
                s = p / 4f;
            }
            else
            {
                s = p / kDOUBLE_PI * Asin(c / a);
            }

            return -(a * Pow(2f, 10f * (t -= 1f)) * Sin((t * d - s) * kDOUBLE_PI / p)) + b;
        }

        public static float ElasticEaseOut(float t, float b, float c, float d)
        {
            if(IsEqual(t, 0f))
            {
                return b;
            }

            if(IsEqual((t /= d), 1f))
            {
                return b + c;
            }
             
            var p = d * 0.3f;
            var a = c; 
            var s = p / 4f;

            return (a * Pow(2f, -10f * t) * Sin((t * d - s) * kDOUBLE_PI / p) + c + b);
        }

        public static float ElasticEaseInOut(float t, float b, float c, float d)
        {
            if(IsEqual(t, 0f))
            {
                return b;
            }

            if(IsEqual((t /= d / 2f), 2f))
            {
                return b + c;
            }

            var p = d * (0.3f * 1.5f);
            var a = c;
            var s = p / 4f;

            if(t < 1f)
            {
                var postFix = a * Pow(2f, 10f * (t -= 1f)); // postIncrement is evil
                return -0.5f * (postFix * Sin((t * d - s) * kDOUBLE_PI / p)) + b;
            }

            var postFix2 = a * Pow(2f, -10f * (t -= 1f)); // postIncrement is evil
            return postFix2 * Sin((t * d - s) * kDOUBLE_PI / p) * 0.5f + c + b;
        }

        #endregion

        // Retracts the motion of an animation slightly before it begins to animate in the path indicated.
        // https://msdn.microsoft.com/en-us/library/system.windows.media.animation.backease(v=vs.110).aspx
        #region BackEase

        public static float BackEaseIn(float t, float b, float c, float d)
        {
            var s = 1.70158f;
            return c * (t /= d) * t * ((s + 1f) * t - s) + b;
        }

        public static float BackEaseOut(float t, float b, float c, float d)
        {
            var s = 1.70158f;
            return c * ((t = t / d - 1f) * t * ((s + 1f) * t + s) + 1f) + b;
        }

        public static float BackEaseInOut(float t, float b, float c, float d)
        {
            var s = 1.70158f;

            if((t /= d / 2f) < 1f)
            {
                return c / 2f * (t * t * (((s *= (1.525f)) + 1f) * t - s)) + b;
            }

            return c / 2f * ((t -= 2f) * t * (((s *= (1.525f)) + 1f) * t + s) + 2f) + b;
        }

        #endregion

        // Represents an easing function that creates an animated bouncing effect.
        // https://msdn.microsoft.com/en-us/library/system.windows.media.animation.bounceease(v=vs.110).aspx
        #region BounceEase

        public static float BounceEaseIn(float t, float b, float c, float d)
        {
            return c - BounceEaseOut(d - t, 0f, c, d) + b;
        }

        public static float BounceEaseOut(float t, float b, float c, float d)
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

        public static float BounceEaseInOut(float t, float b, float c, float d)
        {
            if(t < d / 2f)
            {
                return BounceEaseIn(t * 2f, 0f, c, d) * 0.5f + b;
            }

            return BounceEaseOut(t * 2f - d, 0f, c, d) * 0.5f + c * 0.5f + b;
        }

        #endregion

        public static float OutDanielBounce(float t, float b, float c, float d)
        {
            var s = 5f;
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
            for(int i = 0; i < easeCustom.Count; ++i)
            {
                var aEase = easeCustom[i];

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
