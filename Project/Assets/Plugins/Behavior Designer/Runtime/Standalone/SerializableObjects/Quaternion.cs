using System;

namespace BehaviorDesigner.Runtime.Standalone
{
    public struct Quaternion
    {
        Vector4 _values;

        public static Quaternion identity { get { return new Quaternion(0, 0, 0, 1); }}

        public float x { get { return _values.x; } set { _values.x = value; } }

        public float y { get { return _values.y; } set { _values.y = value; } }

        public float z { get { return _values.z; } set { _values.z = value; } }

        public float w { get { return _values.w; } set { _values.w = value; } }

        Vector3 xyz { get { return new Vector3(x, y, z); } set{ x = value.x; y = value.y; z = value.z; } }

        Vector3 xy { get { return new Vector2(x, y); } set{ x = value.x; y = value.y; } }

        public Quaternion(float x, float y, float z, float w)
        {
            _values = Vector4.zero;
            
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quaternion(Vector4 values)
        {
            _values = values;
        }

        public Quaternion(Quaternion other)
        {
            _values = other._values;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", x, y, z, w);
        }

        public override bool Equals(object obj)
        {
            return Equals((Vector4)obj);
        }

        public bool Equals(Vector4 c2)
        {
            Vector4 c1 = this._values;

            return (Math.Abs(c1.x - c2.x) < Mathf.Epsilon &&
            Math.Abs(c1.y - c2.y) < Mathf.Epsilon &&
            Math.Abs(c1.z - c2.z) < Mathf.Epsilon &&
            Math.Abs(c1.w - c2.w) < Mathf.Epsilon);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
        }

        public static bool operator ==(Quaternion c1, Quaternion c2)
        {  
            return (Math.Abs(c1.x - c2.x) < Mathf.Epsilon &&
            Math.Abs(c1.y - c2.y) < Mathf.Epsilon &&
            Math.Abs(c1.z - c2.z) < Mathf.Epsilon &&
            Math.Abs(c1.w - c2.w) < Mathf.Epsilon);
        }

        public static bool operator !=(Quaternion c1, Quaternion c2)
        {  
            return !(c1 == c2);
        }

        public static Quaternion operator *(Quaternion c1, Quaternion c2)
        {  
            return Quaternion.Multiply(c1, c2);
        }

        public static Vector3 operator *(Quaternion c1, Vector3 c2)
        {  
            return Quaternion.RotateVector(c1, c2);
        }

        public static Quaternion Normalize(Quaternion quad)
        {
            float lenSQ = quad._values.sqrMagnitude;
            return new Quaternion(quad._values / lenSQ);
        }

        public static Quaternion AngleAxis(float angle, Vector3 axis)
        {
            angle *= 0.5f;
            float s = (float)Math.Sin((double)angle);
            Quaternion quad = new Quaternion(new Vector4(axis * s, (float)Math.Cos((double)angle)));
            return Quaternion.Normalize(quad);
        }

        static Quaternion Multiply(Quaternion q1, Quaternion q2)
        {
            Quaternion res = new Quaternion();
            
            res.w   = q1.w*q2.w - Vector3.Dot(q1.xyz, q2.xyz);
            res.xyz = q1.w*q2.xyz + q2.w*q1.xyz + Vector3.Cross(q1.xyz, q2.xyz);
            return Normalize(res);
        }

        static Vector3 RotateVector(Quaternion q, Vector3 v)
        {
            Vector3 t = 2f * Vector3.Cross(q.xyz, v);
            return v + q.w*t + Vector3.Cross(q.xyz, t);
        }
    }
}
