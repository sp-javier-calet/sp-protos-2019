namespace BehaviorDesigner.Runtime.Standalone
{
    public static class Random
    {
        static System.Random RandomGenerator = new System.Random();

        public static int seed { set { RandomGenerator = new System.Random(value); } }

        public static void InitState(int seed)
        {
            RandomGenerator = new System.Random(seed);
        }

        public static int Range(int min, int max)
        {
            int value = (int)System.Math.Floor(System.Math.Round((double)min + Random01() * (max - min)));
            return Mathf.Clamp(value, min, max - 1);
        }

        public static float Range(float min, float max)
        {
            return min + Random01() * (max - min);
        }

        public static float Random01()
        {
            int randInt = RandomGenerator.Next(int.MinValue, int.MaxValue);
            double a = ((double)randInt) - ((double)int.MinValue);
            double b = ((double)int.MaxValue) - ((double)int.MinValue);
            return (float)(a / b);
        }

        public static float Random01(float seed)
        {
            return Mathf.Frac(7.54321f * (float)System.Math.Sin(seed * 268.4575f));
        }
    }
}
