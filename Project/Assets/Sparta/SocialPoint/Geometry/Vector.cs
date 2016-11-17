
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

    // UnityVector adapter
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


    // TODO Move to file
    public partial struct Quat
    {
        float _x;
        float _y;
        float _z;
        float _w;

        public Quat(float x, float y, float z, float w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }
    }

    // UnityVector adapter
    public partial struct Quat
    {
        public static Quat Convert(UnityEngine.Quaternion q)
        {
            return new Quat(q.x, q.y, q.z, q.w);
        }

        public static implicit operator Quat(UnityEngine.Quaternion q)
        {
            return new Quat(q.x, q.y, q.z, q.w);
        }

        public static implicit operator UnityEngine.Quaternion(Quat q)
        {
            return new UnityEngine.Quaternion(q._x, q._y, q._z, q._w);
        }
    }
}
