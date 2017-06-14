using System;

namespace BehaviorDesigner.Runtime.Standalone
{
    public static class Mathf
    {
        public static readonly float Epsilon = 1e-3f;

        public static readonly float Deg2Rad = ((float)Math.PI)/180f;
        public static readonly float Rad2Deg = 1f/Deg2Rad;

        public static float Lerp(float c1, float c2, float lerpFactor)
        {  
            return c1 * (1f - lerpFactor) + c2 * lerpFactor;
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        public static int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        public static float Hash(string value)
        {  
            return value.GetHashCode();
        }

        public static float Frac(float value)
        {
            value = Math.Abs(value);
            return value - ((float)Math.Floor(value));
        }
    }
}
