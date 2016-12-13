namespace SocialPoint.Geometry
{
    public partial struct Vector
    {
        float _x;
        float _y;
        float _z;

        public Vector(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }
    }

    // Unity Vector3 adapter
    public partial struct Vector
    {
        public static Vector Convert(UnityEngine.Vector3 v)
        {
            return new Vector(v.x, v.y, v.z);
        }

        public static implicit operator Vector(UnityEngine.Vector3 v)
        {
            return new Vector(v.x, v.y, v.z);
        }

        public static implicit operator UnityEngine.Vector3(Vector v)
        {
            return new UnityEngine.Vector3(v._x, v._y, v._z);
        }
    }

    // Unity Vector2 adapter
    public partial struct Vector
    {
        public static Vector Convert(UnityEngine.Vector2 v)
        {
            return new Vector(v.x, v.y, 0);
        }

        public static implicit operator Vector(UnityEngine.Vector2 v)
        {
            return new Vector(v.x, v.y, 0);
        }

        public static implicit operator UnityEngine.Vector2(Vector v)
        {
            return new UnityEngine.Vector2(v._x, v._y);
        }
    }
}
