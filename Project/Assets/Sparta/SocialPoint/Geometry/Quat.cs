namespace SocialPoint.Geometry
{
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

    // Unity Quaternion adapter
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