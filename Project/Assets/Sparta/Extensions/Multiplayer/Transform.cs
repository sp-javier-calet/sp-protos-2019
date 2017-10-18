using System;
using Jitter.LinearMath;
using SocialPoint.IO;
using SocialPoint.Physics;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public partial class Transform : IEquatable<Transform>, IPoolCloneable, ICopyable
    {
        public JVector Position;

        public JQuaternion Rotation;

        public JVector Scale;

        public Transform(JVector p, JQuaternion r, JVector s)
        {
            Position = p;
            Rotation = r;
            Scale = s;
        }

        public Transform(JVector p, JQuaternion r) : this(p, r, JVector.One)
        {
        }

        public Transform(JVector p) : this(p, JQuaternion.Identity)
        {
        }

        public Transform() : this(JVector.Zero)
        {
        }

        public void Copy(object other)
        {
            var trans = other as Transform;
            Position = trans.Position;
            Rotation = trans.Rotation;
            Scale = trans.Scale;
        }

        public object Clone(ObjectPool pool = null)
        {
            var t = pool == null ? new Transform() : pool.Get<Transform>();
            t.Copy(this);
            return t;
        }

        public void Dispose()
        {
            Reset();
        }

        public void Reset()
        {
            Position = JVector.Zero;
            Scale = JVector.One;
            Rotation = JQuaternion.Identity;
        }

        public override bool Equals(object obj)
        {
            var go = (Transform)obj;
            if((object)go == null)
            {
                return false;
            }
            return Compare(this, go);
        }

        public bool Equals(Transform go)
        {
            if((object)go == null)
            {
                return false;
            }
            return Compare(this, go);
        }

        public void MoveTowards(JVector targetPos, float speed, float dt)
        {
            Position += GetLinearMovementVector(targetPos, speed, dt);
        }

        public JVector GetLinearMovementVector(JVector targetPos, float speed, float dt)
        {
            var direction = targetPos - Position;
            var distanceSqr = direction.LengthSquared();

            var normalStep = speed * dt;
            var normalStepSqr = normalStep * normalStep;

            var arrive = (distanceSqr <= normalStepSqr);
            var step = arrive ? (float)Math.Sqrt(distanceSqr) : normalStep;

            if(distanceSqr > 0f)
            {
                direction.Normalize();
            }
            return direction * step;
        }

        public void LookAt(JVector targetPos)
        {
            var direction = targetPos - Position;
            if(direction.LengthSquared() <= 0.0001f)
            {
                return;
            }
            var yRotation = (float)Math.Atan2(direction.X, direction.Z);
            JQuaternion.CreateFromYawPitchRoll(yRotation, 0f, 0f, out Rotation);
        }

        public bool MoveLookTowards(JVector targetPos, float speed, float dt, float deltaSqr = 0.01f)
        {
            if(IsNear(targetPos, deltaSqr))
            {
                return false;
            }
            LookAt(targetPos);
            MoveTowards(targetPos, speed, dt);
            return true;
        }

        public bool IsNear(JVector pos, float deltaSqr = 0.1f)
        {
            var dir = pos - Position;
            var distSqr = Scale * dir.LengthSquared();
            return distSqr.X <= deltaSqr && distSqr.Y <= deltaSqr && distSqr.Z <= deltaSqr;
        }


        public JVector Forward
        {
            get
            {
                return Rotation.Forward();
            }
        }

        public static bool operator ==(Transform a, Transform b)
        {
            var na = (object)a == null;
            var nb = (object)b == null;
            if(na && nb)
            {
                return true;
            }
            else if(na || nb)
            {
                return false;
            }
            return Compare(a, b);
        }

        public static bool operator !=(Transform a, Transform b)
        {
            return !(a == b);
        }

        static bool Compare(Transform a, Transform b)
        {
            return a.Position.AlmostEquals(b.Position) && a.Rotation.AlmostEquals(b.Rotation) && a.Scale.AlmostEquals(b.Scale);
        }

        public override int GetHashCode()
        {
            var hash = Position.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, Rotation.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, Scale.GetHashCode());
            return hash;
        }

        public static Transform Identity
        {
            get
            {
                return new Transform(JVector.Zero);
            }
        }

        public override string ToString()
        {
            return string.Format("[Transform: Position={0}, Rotation={1}, Scale={2}]", Position, Rotation, Scale);
        }
    }

    public class TransformSerializer : IDiffWriteSerializer<Transform>
    {
        public static readonly TransformSerializer Instance = new TransformSerializer();

        public void Compare(Transform newObj, Transform oldObj, Bitset dirty)
        {
            dirty.Set(newObj.Position != oldObj.Position);
            dirty.Set(newObj.Rotation != oldObj.Rotation);
            dirty.Set(newObj.Scale != oldObj.Scale);
        }

        public void Serialize(Transform newObj, IWriter writer)
        {
            JVectorSerializer.Instance.Serialize(newObj.Position, writer);
            JQuaternionSerializer.Instance.Serialize(newObj.Rotation, writer);
            JVectorSerializer.Instance.Serialize(newObj.Scale, writer);
        }

        public void Serialize(Transform newObj, Transform oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                JVectorSerializer.Instance.Serialize(newObj.Position, oldObj.Position, writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                JQuaternionSerializer.Instance.Serialize(newObj.Rotation, oldObj.Rotation, writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                JVectorSerializer.Instance.Serialize(newObj.Scale, oldObj.Scale, writer);
            }
        }
    }

    public class TransformParser : IDiffReadParser<Transform>
    {
        public static readonly TransformParser Instance = new TransformParser();

        public Transform Parse(IReader reader)
        {
            var obj = new Transform();
            obj.Position = JVectorParser.Instance.Parse(reader);
            obj.Rotation = JQuaternionParser.Instance.Parse(reader);
            obj.Scale = JVectorParser.Instance.Parse(reader);
            return obj;
        }

        public int GetDirtyBitsSize(Transform obj)
        {
            return 3;
        }

        public Transform Parse(Transform obj, IReader reader, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                obj.Position = JVectorParser.Instance.Parse(obj.Position, reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Rotation = JQuaternionParser.Instance.Parse(obj.Rotation, reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Scale = JVectorParser.Instance.Parse(obj.Scale, reader);
            }
            return obj;
        }
    }

    public class TransformShortSerializer : IDiffWriteSerializer<Transform>
    {
        public static readonly TransformShortSerializer Instance = new TransformShortSerializer();

        public void Compare(Transform newObj, Transform oldObj, Bitset dirty)
        {
            dirty.Set(newObj.Position != oldObj.Position);
            dirty.Set(newObj.Rotation != oldObj.Rotation);
            dirty.Set(newObj.Scale != oldObj.Scale);
        }

        public void Serialize(Transform newObj, IWriter writer)
        {
            JVectorShortSerializer.Instance.Serialize(newObj.Position, writer);
            JQuaternionShortSerializer.Instance.Serialize(newObj.Rotation, writer);
            JVectorShortSerializer.Instance.Serialize(newObj.Scale, writer);
        }

        public void Serialize(Transform newObj, Transform oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                JVectorShortSerializer.Instance.Serialize(newObj.Position, oldObj.Position, writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                JQuaternionShortSerializer.Instance.Serialize(newObj.Rotation, oldObj.Rotation, writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                JVectorShortSerializer.Instance.Serialize(newObj.Scale, oldObj.Scale, writer);
            }
        }
    }

    public class TransformShortParser : IDiffReadParser<Transform>
    {
        public static readonly TransformShortParser Instance = new TransformShortParser();

        public Transform Parse(IReader reader)
        {
            var obj = new Transform();
            obj.Position = JVectorShortParser.Instance.Parse(reader);
            obj.Rotation = JQuaternionShortParser.Instance.Parse(reader);
            obj.Scale = JVectorShortParser.Instance.Parse(reader);
            return obj;
        }

        public int GetDirtyBitsSize(Transform obj)
        {
            return 3;
        }

        public Transform Parse(Transform obj, IReader reader, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                obj.Position = JVectorShortParser.Instance.Parse(obj.Position, reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Rotation = JQuaternionShortParser.Instance.Parse(obj.Rotation, reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Scale = JVectorShortParser.Instance.Parse(obj.Scale, reader);
            }
            return obj;
        }
    }
}
