using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.GUIAnimation
{
    // Class that have all the predefined easing types

    public enum EaseType
    {
        easelinear,

        easeInSine,
        easeOutSine,
        easeInOutSine,

        easeInQuad,
        easeOutQuad,
        easeInOutQuad,

        easeInCubic,
        easeOutCubic,
        easeInOutCubic,

        easeInQuart,
        easeOutQuart,
        easeInOutQuart,

        easeInQuint,
        easeOutQuint,
        easeInOutQuint,

        easeInExpo,
        easeOutExpo,
        easeInOutExpo,

        easeInCirc,
        easeOutCirc,
        easeInOutCirc,

        easeInElastic,
        easeOutElastic,
        easeInOutElastic,

        easeInBack,
        easeOutBack,
        easeInOutBack,

        easeInBounce,
        easeOutBounce,
        easeInOutBounce,

        easeOutDanielBounce,
    }

    public interface IEase
    {
        float ease(float t, float b, float c, float d);
    }

    public interface IEaseCustom
    {
        float ease(float t, float d, List<Vector2> timeValues);
    }

    public class EaseManager
    {
        static EaseManager _instance;

        public static EaseManager GetInstance()
        {
            if(_instance == null)
            {
                _instance = new EaseManager();
                _instance.Init();
            }
            return _instance;
        }

        Dictionary<EaseType, IEase> _easeTypes = new Dictionary<EaseType, IEase>();

        public void Add(EaseType type, IEase ease)
        {
            _easeTypes.Add(type, ease);
        }

        public IEase Get(EaseType type)
        {
            IEase ease;
            if(_easeTypes.TryGetValue(type, out ease))
            {
                return ease;
            }
            else
            {
                return null;
            }
        }

        IEaseCustom _easeCustom = new easeCustom();

        public IEaseCustom GetCustom()
        {
            return _easeCustom;
        }

        void Init()
        {
            EaseManager.GetInstance().Add(EaseType.easelinear, new easelinear());

            EaseManager.GetInstance().Add(EaseType.easeInSine, new easeInSine());
            EaseManager.GetInstance().Add(EaseType.easeOutSine, new easeOutSine());
            EaseManager.GetInstance().Add(EaseType.easeInOutSine, new easeInOutSine());

            EaseManager.GetInstance().Add(EaseType.easeInQuad, new easeInQuad());
            EaseManager.GetInstance().Add(EaseType.easeOutQuad, new easeOutQuad());
            EaseManager.GetInstance().Add(EaseType.easeInOutQuad, new easeInOutQuad());

            EaseManager.GetInstance().Add(EaseType.easeInCubic, new easeInCubic());
            EaseManager.GetInstance().Add(EaseType.easeOutCubic, new easeOutCubic());
            EaseManager.GetInstance().Add(EaseType.easeInOutCubic, new easeInOutCubic());

            EaseManager.GetInstance().Add(EaseType.easeInQuart, new easeInQuart());
            EaseManager.GetInstance().Add(EaseType.easeOutQuart, new easeOutQuart());
            EaseManager.GetInstance().Add(EaseType.easeInOutQuart, new easeInOutQuart());

            EaseManager.GetInstance().Add(EaseType.easeInQuint, new easeInQuint());
            EaseManager.GetInstance().Add(EaseType.easeOutQuint, new easeOutQuint());
            EaseManager.GetInstance().Add(EaseType.easeInOutQuint, new easeInOutQuint());

            EaseManager.GetInstance().Add(EaseType.easeInExpo, new easeInExpo());
            EaseManager.GetInstance().Add(EaseType.easeOutExpo, new easeOutExpo());
            EaseManager.GetInstance().Add(EaseType.easeInOutExpo, new easeInOutExpo());

            EaseManager.GetInstance().Add(EaseType.easeInCirc, new easeInCirc());
            EaseManager.GetInstance().Add(EaseType.easeOutCirc, new easeOutCirc());
            EaseManager.GetInstance().Add(EaseType.easeInOutCirc, new easeInOutCirc());

            EaseManager.GetInstance().Add(EaseType.easeInElastic, new easeInElastic());
            EaseManager.GetInstance().Add(EaseType.easeOutElastic, new easeOutElastic());
            EaseManager.GetInstance().Add(EaseType.easeInOutElastic, new easeInOutElastic());

            EaseManager.GetInstance().Add(EaseType.easeInBack, new easeInBack());
            EaseManager.GetInstance().Add(EaseType.easeOutBack, new easeOutBack());
            EaseManager.GetInstance().Add(EaseType.easeInOutBack, new easeInOutBack());

            EaseManager.GetInstance().Add(EaseType.easeInBounce, new easeInBounce());
            EaseManager.GetInstance().Add(EaseType.easeOutBounce, new easeOutBounce());
            EaseManager.GetInstance().Add(EaseType.easeInOutBounce, new easeInOutBounce());

            EaseManager.GetInstance().Add(EaseType.easeOutDanielBounce, new easeOutDanielBounce());
        }
    }

    // t: time, b: startValue, c: changeInValue, d: duration
    public class easelinear : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return AnimationMathUtility.Lerp(b, b + c, t / d);
        }
    }

    //----
    public class easeInSine : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return -c * Mathf.Cos(t / d * (Mathf.PI / 2f)) + c + b;
        }
    }

    public class easeOutSine : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return c * Mathf.Sin(t / d * (Mathf.PI / 2f)) + b;
        }
    }

    public class easeInOutSine : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return -c / 2f * (Mathf.Cos(Mathf.PI * t / d) - 1f) + b;
        }
    }

    //----
    public class easeInQuad : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return c * (t /= d) * t + b;
        }
    }

    public class easeOutQuad : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return -c * (t /= d) * (t - 2f) + b;
        }
    }

    public class easeInOutQuad : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
                return c / 2f * t * t + b;
            return -c / 2f * ((--t) * (t - 2f) - 1f) + b;
        }
    }

    //----
    public class easeInCubic : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t + b;
        }
    }

    public class easeOutCubic : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1f) * t * t + 1f) + b;
        }
    }

    public class easeInOutCubic : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
                return c / 2f * t * t * t + b;
            return c / 2f * ((t -= 2f) * t * t + 2f) + b;
        }
    }

    //----
    public class easeInQuart : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t * t + b;
        }
    }

    public class easeOutQuart : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return -c * ((t = t / d - 1f) * t * t * t - 1f) + b;
        }
    }

    public class easeInOutQuart : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
                return c / 2f * t * t * t * t + b;
            return -c / 2f * ((t -= 2f) * t * t * t - 2f) + b;
        }
    }

    //----
    public class easeInQuint : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t * t * t + b;
        }
    }

    public class easeOutQuint : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1f) * t * t * t * t + 1f) + b;
        }
    }

    public class easeInOutQuint : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
                return c / 2f * t * t * t * t * t + b;
            return c / 2f * ((t -= 2f) * t * t * t * t + 2f) + b;
        }
    }

    //----
    public class easeInExpo : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return (t == 0f) ? b : c * Mathf.Pow(2f, 10f * (t / d - 1f)) + b;
        }
    }

    public class easeOutExpo : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return (t == d) ? b + c : c * (-Mathf.Pow(2f, -10f * t / d) + 1f) + b;
        }
    }

    public class easeInOutExpo : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            if(t == 0f)
                return b;
            if(t == d)
                return b + c;
            if((t /= d / 2f) < 1f)
                return c / 2f * Mathf.Pow(2f, 10f * (t - 1f)) + b;
            return c / 2 * (-Mathf.Pow(2f, -10f * --t) + 2f) + b;
        }
    }

    //----
    public class easeInCirc : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return -c * (Mathf.Sqrt(1f - (t /= d) * t) - 1f) + b;
        }
    }

    public class easeOutCirc : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            return c * Mathf.Sqrt(1f - (t = t / d - 1f) * t) + b;
        }
    }

    public class easeInOutCirc : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            if((t /= d / 2f) < 1f)
                return -c / 2f * (Mathf.Sqrt(1f - t * t) - 1f) + b;
            return c / 2 * (Mathf.Sqrt(1 - (t -= 2) * t) + 1) + b;
        }
    }

    //----
    public class easeInElastic : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            float s = 1.70158f;
            float p = 0f;
            float a = c;
            if(AnimationMathUtility.IsEquals(t, 0f))
                return b;
            if(AnimationMathUtility.IsEquals((t /= d), 1f))
                return b + c;
            if(!AnimationMathUtility.IsZero(p))
                p = d * 0.3f;
            if(a < Mathf.Abs(c))
            {
                a = c;
                s = p / 4f;
            }
            else
                s = p / (2f * Mathf.PI) * Mathf.Asin(c / a);
            return -(a * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t * d - s) * (2f * Mathf.PI) / p)) + b;
        }
    }

    public class easeOutElastic : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            if(t == 0d)
                return b;
            if((t /= d) == 1f)
                return b + c;  
            float p = d * 0.3f;
            float a = c; 
            float s = p / 4f;
            return (a * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * d - s) * (2f * Mathf.PI) / p) + c + b);	
        }
    }

    public class easeInOutElastic : IEase
    {
        public float ease(float t, float b, float c, float d)
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
                float postFix = a * Mathf.Pow(2f, 10f * (t -= 1f)); // postIncrement is evil
                return -0.5f * (postFix * Mathf.Sin((t * d - s) * (2f * Mathf.PI) / p)) + b;
            } 
            float postFix2 = a * Mathf.Pow(2f, -10f * (t -= 1f)); // postIncrement is evil
            return postFix2 * Mathf.Sin((t * d - s) * (2f * Mathf.PI) / p) * 0.5f + c + b;
        }
    }

    //----
    public class easeInBack : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            float s = 1.70158f;
            return c * (t /= d) * t * ((s + 1f) * t - s) + b;
        }
    }

    public class easeOutBack : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            float s = 1.70158f;
            return c * ((t = t / d - 1f) * t * ((s + 1f) * t + s) + 1f) + b;
        }
    }

    public class easeInOutBack : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            float s = 1.70158f; 
            if((t /= d / 2f) < 1f)
                return c / 2f * (t * t * (((s *= (1.525f)) + 1f) * t - s)) + b;
            return c / 2f * ((t -= 2f) * t * (((s *= (1.525f)) + 1f) * t + s) + 2f) + b;
        }
    }

    //----
    public class easeInBounce : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            easeOutBounce ease = new easeOutBounce();
            return c - ease.ease(d - t, 0f, c, d) + b;
        }
    }

    public class easeOutBounce : IEase
    {
        public float ease(float t, float b, float c, float d)
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
    }

    public class easeInOutBounce : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            easeInBounce easein = new easeInBounce();
            easeOutBounce easeout = new easeOutBounce();

            if(t < d / 2f)
                return easein.ease(t * 2f, 0f, c, d) * 0.5f + b;
            return easeout.ease(t * 2f - d, 0f, c, d) * 0.5f + c * 0.5f + b;
        }
    }

    //----
    public class easeCustom : IEaseCustom
    {
        public float ease(float t, float d, List<Vector2> timeValues)
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

            endIdx = Mathf.Min(startIdx + 1, timeValues.Count - 1);

            Vector2 start = timeValues[startIdx];
            Vector2 end = timeValues[endIdx];

            float deltaT = (end.x - start.x);
            float lerpValue = 0f;

            if(Mathf.Abs(deltaT) < 1e-5f)
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
    }

    //----
    public class easeOutDanielBounce : IEase
    {
        public float ease(float t, float b, float c, float d)
        {
            float s = 5f;
            return c * ((t = t / d - 1f) * t * ((s + 1f) * t + s) + 1f) + b;
        }
    }
}
