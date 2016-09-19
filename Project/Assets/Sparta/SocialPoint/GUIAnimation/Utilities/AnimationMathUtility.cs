using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public static class AnimationMathUtility
    {
        public static double Abs(double val)
        {
            return val < 0 ? -val : val;
        }

        public static float kEpsilon = 1e-4f;

        public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
        {
            return from * (1f - t) + to * t;
        }

        public static float Lerp(float from, float to, float t)
        {
            return from * (1f - t) + to * t;
        }

        public static float SmoothStep(float from, float to, float t)
        {
            return Mathf.Clamp01((t - from) / (to - from));
        }

        public static bool IsZero(float value)
        {
            return Equals(value, 0f);
        }

        public static bool IsEquals(float a, float b)
        {
            return Mathf.Abs(a - b) <= kEpsilon;
        }
    }
}
