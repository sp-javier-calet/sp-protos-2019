using System;

namespace BehaviorDesigner.Runtime.Standalone
{
    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector4(Vector3 xyz, float w)
        {
            this.x = xyz.x;
            this.y = xyz.y;
            this.z = xyz.z;
            this.w = w;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", x, y, z, w);
        }

        public override bool Equals(object obj)
        {
            return Equals( (Vector4) obj );
        }

        public bool Equals(Vector4 c2)
        {
            Vector4 c1 = this;

            return (Math.Abs(c1.x - c2.x) < Mathf.Epsilon &&
                Math.Abs(c1.y - c2.y) < Mathf.Epsilon &&
                Math.Abs(c1.z - c2.z) < Mathf.Epsilon &&
                Math.Abs(c1.w - c2.w) < Mathf.Epsilon);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
        }

        public static bool operator ==(Vector4 c1, Vector4 c2)
        {  
            return (Math.Abs(c1.x - c2.x) < Mathf.Epsilon &&
                Math.Abs(c1.y - c2.y) < Mathf.Epsilon &&
                Math.Abs(c1.z - c2.z) < Mathf.Epsilon &&
                Math.Abs(c1.w - c2.w) < Mathf.Epsilon);
        }

        public static bool operator !=(Vector4 c1, Vector4 c2)
        {  
            return !(c1 == c2);
        }

        public static Vector4 operator +(Vector4 c1, Vector4 c2)
        {  
            return new Vector4(c1.x + c2.x, c1.y + c2.y, c1.z + c2.z, c1.w + c2.w);  
        }

        public static Vector4 operator -(Vector4 c1, Vector4 c2)
        {  
            return new Vector4(c1.x - c2.x, c1.y - c2.y, c1.z - c2.z, c1.w - c2.w);  
        }

        public static Vector4 operator *(Vector4 c1, Vector4 c2)
        {  
            return new Vector4(c1.x * c2.x, c1.y * c2.y, c1.z * c2.z, c1.w * c2.w);  
        }

        public static Vector4 operator /(Vector4 c1, Vector4 c2)
        {  
            return new Vector4(c1.x / c2.x, c1.y / c2.y, c1.z / c2.z, c1.w / c2.w);
        }

        public static Vector4 operator *(Vector4 c1, float c2)
        {  
            return new Vector4(c1.x * c2, c1.y * c2, c1.z * c2, c1.w * c2);
        }

        public static Vector4 Scale(Vector4 c1, Vector4 c2)
        {
            return c1 * c2;
        }

        public static Vector4 operator /(Vector4 c1, float c2)
        {  
            return new Vector4(c1.x / c2, c1.y / c2, c1.z / c2, c1.w / c2);
        }

        public static Vector4 zero
        {  
            get
            {
                return new Vector4(0f, 0f, 0f, 0f);
            }
        }

        public static Vector4 one
        {  
            get
            {
                return new Vector4(1f, 1f, 1f, 1f);
            }
        }

        public float sqrMagnitude
        {
            get
            {
                return this.Dot(this);
            }
        }

        public float magnitude
        {
            get
            {
                return (float)Math.Sqrt((double)this.sqrMagnitude);
            }
        }

        public static Vector4 Normalize(Vector4 v)
        {
            v.Normalize();
            return v;
        }

        public void Normalize()
        {
            this = this / this.magnitude;
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            return (b - a).magnitude;
        }

        public Vector4 normalized
        {
            get
            {
                return this / this.magnitude;
            }
        }

        public float Dot(Vector4 c2)
        {
            return Vector4.Dot(this, c2);
        }

        public static float Dot(Vector4 c1, Vector4 c2)
        {
            return c1.x * c2.x + c1.y * c2.y + c1.z * c2.z + c1.w * c2.w;
        }

        public static Vector4 ClampMagnitude(Vector4 c1, float magnitude)
        {
            float clampedMagnitude = Mathf.Clamp(c1.magnitude, 0f, magnitude);
            return c1.normalized * clampedMagnitude;
        }

        public static Vector4 Lerp(Vector4 c1, Vector4 c2, float lerpFactor)
        {  
            return new Vector4(
                c1.x * (1f - lerpFactor) + c2.x * lerpFactor,
                c1.y * (1f - lerpFactor) + c2.y * lerpFactor,
                c1.z * (1f - lerpFactor) + c2.z * lerpFactor,
                c1.w * (1f - lerpFactor) + c2.w * lerpFactor);
        }

        public static Vector4 MoveTowards(Vector4 start, Vector4 end, float distance)
        {
            Vector4 dirToTarget = end - start;
            float realDist = dirToTarget.magnitude;
            distance = (float)Math.Min(realDist, distance);

            if(dirToTarget.sqrMagnitude < 1e-4f)
            {
                return end;
            }

            return start + dirToTarget.normalized * distance;
        }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public static Vector3 right { get { return new Vector3(1,0,0); } }
        public static Vector3 up { get { return new Vector3(0,1,0); } }
        public static Vector3 forward { get { return new Vector3(0,0,1); } }

        public static implicit operator Vector3(Vector2 value) { return new Vector3(value.x, value.y); }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(float x, float y)
            : this(x, y, 0f)
        {
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            return Equals( (Vector4) obj );
        }

        public bool Equals(Vector3 c2)
        {
            Vector3 c1 = this;

            return (Math.Abs(c1.x - c2.x) < Mathf.Epsilon &&
                Math.Abs(c1.y - c2.y) < Mathf.Epsilon &&
                Math.Abs(c1.z - c2.z) < Mathf.Epsilon);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }

        public static bool operator ==(Vector3 c1, Vector3 c2)
        {  
            return (Math.Abs(c1.x - c2.x) < Mathf.Epsilon &&
                Math.Abs(c1.y - c2.y) < Mathf.Epsilon &&
                Math.Abs(c1.z - c2.z) < Mathf.Epsilon
            );
        }

        public static bool operator !=(Vector3 c1, Vector3 c2)
        {  
            return !(c1 == c2);
        }

        public static Vector3 operator +(Vector3 c1, Vector3 c2)
        {  
            return new Vector3(c1.x + c2.x, c1.y + c2.y, c1.z + c2.z);  
        }

        public static Vector3 operator -(Vector3 c1, Vector3 c2)
        {  
            return new Vector3(c1.x - c2.x, c1.y - c2.y, c1.z - c2.z);  
        }

        public static Vector3 operator *(Vector3 c1, Vector3 c2)
        {  
            return new Vector3(c1.x * c2.x, c1.y * c2.y, c1.z * c2.z);  
        }

        public static Vector3 Scale(Vector3 c1, Vector3 c2)
        {
            return c1 * c2;
        }

        public static Vector3 operator /(Vector3 c1, Vector3 c2)
        {  
            return new Vector3(c1.x / c2.x, c1.y / c2.y, c1.z / c2.z);  
        }

        public static Vector3 operator *(Vector3 c1, float c2)
        {  
            return new Vector3(c1.x * c2, c1.y * c2, c1.z * c2);  
        }

        public static Vector3 operator *(float c2, Vector3 c1)
        {  
            return new Vector3(c1.x * c2, c1.y * c2, c1.z * c2);  
        }

        public static Vector3 operator /(Vector3 c1, float c2)
        {  
            return new Vector3(c1.x / c2, c1.y / c2, c1.z / c2);
        }

        public static Vector3 zero
        {  
            get
            {
                return new Vector3(0f, 0f, 0f);
            }
        }

        public static Vector3 one
        {  
            get
            {
                return new Vector3(1f, 1f, 1f);
            }
        }

        public float sqrMagnitude
        {
            get
            {
                return this.Dot(this);
            }
        }

        public float magnitude
        {
            get
            {
                return (float)Math.Sqrt((double)this.sqrMagnitude);
            }
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            return (b - a).magnitude;
        }

        public Vector3 normalized
        {
            get
            {
                return this / this.magnitude;
            }
        }

        public static Vector3 Normalize(Vector3 v)
        {
            v.Normalize();
            return v;
        }

        public void Normalize()
        {
            this = this / this.magnitude;
        }

        public float Dot(Vector3 c2)
        {
            return Vector3.Dot(this, c2);
        }

        public static float Dot(Vector3 c1, Vector3 c2)
        {
            return c1.x * c2.x + c1.y * c2.y + c1.z * c2.z;
        }

        public static Vector3 ClampMagnitude(Vector3 c1, float magnitude)
        {
            float clampedMagnitude = Mathf.Clamp(c1.magnitude, 0f, magnitude);
            return c1.normalized * clampedMagnitude;
        }

        public static float Angle(Vector3 front, Vector3 other)
        {
            front.Normalize();
            other.Normalize();
            Vector3 right = new Vector3(front.z, front.y, -front.x);

            float x = Vector3.Dot(front, other);
            float y = Vector3.Dot(right, other);

            return (float) Math.Atan2((double) y, (double) (x));
        }

        public static Vector3 Lerp(Vector3 c1, Vector3 c2, float lerpFactor)
        {  
            return new Vector3(
                c1.x * (1f - lerpFactor) + c2.x * lerpFactor,
                c1.y * (1f - lerpFactor) + c2.y * lerpFactor,
                c1.z * (1f - lerpFactor) + c2.z * lerpFactor);
        }

        public static Vector3 MoveTowards(Vector3 start, Vector3 end, float distance)
        {
            Vector3 dirToTarget = end - start;
            float realDist = dirToTarget.magnitude;
            distance = (float)Math.Min(realDist, distance);

            if(dirToTarget.sqrMagnitude < 1e-4f)
            {
                return end;
            }

            return start + dirToTarget.normalized * distance;
        }

        public static Vector3 Cross(Vector3 left, Vector3 right)
        {
            Vector3 result = new Vector3();
            result.x = left.y * right.z - left.z * right.y;
            result.y = -left.x * right.z + left.z * right.x;
            result.z = left.x * right.y - left.y * right.x;
            return result;
        }

        public static Vector3 RotateTowards(Vector3 left, Vector3 right, float maxRadians, float maxMagnitude)
        {
            left.Normalize();
            right.Normalize();

            // Clamp angle
            float angle = Vector3.Angle(left, right);
            angle = Math.Min( angle, maxRadians );

            // Rotate
            Vector3 axis = Vector3.Cross(left, right);
            Quaternion quat = Quaternion.AngleAxis(angle, axis);
            return quat * left;
        }
    }

    public struct Vector2
    {
        public float x;
        public float y;

        public static Vector2 right { get { return new Vector2(1,0); } }
        public static Vector2 up { get { return new Vector2(0,1); } }

        public static implicit operator Vector2(Vector3 value) { return new Vector2(value.x, value.y); }

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}})", x, y);
        }

        public override bool Equals(object obj)
        {
            return Equals( (Vector4) obj );
        }

        public bool Equals(Vector3 c2)
        {
            Vector2 c1 = this;

            return (Math.Abs(c1.x - c2.x) < Mathf.Epsilon && 
                Math.Abs(c1.y - c2.y) < Mathf.Epsilon);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public static bool operator ==(Vector2 c1, Vector2 c2)
        {  
            return (Math.Abs(c1.x - c2.x) < Mathf.Epsilon && 
                Math.Abs(c1.y - c2.y) < Mathf.Epsilon);
        }

        public static bool operator !=(Vector2 c1, Vector2 c2)
        {  
            return !(c1 == c2);
        }

        public static Vector2 operator +(Vector2 c1, Vector2 c2)
        {  
            return new Vector2(c1.x + c2.x, c1.y + c2.y);  
        }

        public static Vector2 operator -(Vector2 c1, Vector2 c2)
        {  
            return new Vector2(c1.x - c2.x, c1.y - c2.y);  
        }

        public static Vector2 operator *(Vector2 c1, Vector2 c2)
        {  
            return new Vector2(c1.x * c2.x, c1.y * c2.y);  
        }

        public static Vector2 operator /(Vector2 c1, Vector2 c2)
        {  
            return new Vector2(c1.x / c2.x, c1.y / c2.y);  
        }

        public static Vector2 operator *(Vector2 c1, float c2)
        {  
            return new Vector2(c1.x * c2, c1.y * c2);  
        }

        public static Vector2 Scale(Vector2 c1, Vector2 c2)
        {
            return c1 * c2;
        }

        public static Vector2 operator /(Vector2 c1, float c2)
        {  
            return new Vector2(c1.x / c2, c1.y / c2);
        }

        public static Vector2 zero
        {  
            get
            {
                return new Vector2(0f, 0f);
            }
        }

        public static Vector2 one
        {  
            get
            {
                return new Vector2(1f, 1f);
            }
        }

        public float sqrMagnitude
        {
            get
            {
                return this.Dot(this);
            }
        }

        public float magnitude
        {
            get
            {
                return (float)Math.Sqrt((double)this.sqrMagnitude);
            }
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            return (b - a).magnitude;
        }

        public static Vector2 Normalize(Vector2 v)
        {
            v.Normalize();
            return v;
        }

        public Vector2 normalized
        {
            get
            {
                return this / this.magnitude;
            }
        }

        public float Dot(Vector2 c2)
        {
            return Vector2.Dot(this, c2);
        }

        public static float Dot(Vector2 c1, Vector2 c2)
        {
            return c1.x * c2.x + c1.y * c2.y;
        }

        public static Vector2 ClampMagnitude(Vector2 c1, float magnitude)
        {
            float clampedMagnitude = Mathf.Clamp(c1.magnitude, 0f, magnitude);
            return c1.normalized * clampedMagnitude;
        }

        public void Normalize()
        {
            this = this / this.magnitude;
        }

        public static Vector2 Lerp(Vector2 c1, Vector2 c2, float lerpFactor)
        {  
            return new Vector2(
                c1.x * (1f - lerpFactor) + c2.x * lerpFactor,
                c1.y * (1f - lerpFactor) + c2.y * lerpFactor);
        }

        public static Vector2 MoveTowards(Vector2 start, Vector2 end, float distance)
        {
            Vector2 dirToTarget = end - start;
            float realDist = dirToTarget.magnitude;
            distance = (float)Math.Min(realDist, distance);

            if(dirToTarget.sqrMagnitude < 1e-4f)
            {
                return end;
            }

            return start + dirToTarget.normalized * distance;
        }
    }
}
