namespace BehaviorDesigner.Runtime.Standalone
{
    public struct Color
    {
        public float r, g, b, a;

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static Color black{ get { return new Color(0, 0, 0, 1); } }

        public static Color white{ get { return new Color(1, 1, 1, 1); } }

        public static Color red{ get { return new Color(1, 0, 0, 1); } }

        public static Color green{ get { return new Color(0, 1, 0, 1); } }

        public static Color blue{ get { return new Color(0, 0, 1, 1); } }
    }
}