using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Utils;
using Jitter.LinearMath;
using Jitter.Collision;
using Jitter.Collision.Shapes;

namespace SocialPoint.Physics
{
    public interface IPhysicsShape : INetworkShareable, ICloneable, ICopyable, IEquatable<IPhysicsShape>
    {
        Shape CollisionShape { get; }
    }

    public static class PhysicsShapeType
    {
        public const byte Box = 1;
        public const byte Sphere = 2;
        public const byte Capsule = 3;
        public const byte Cylinder = 4;
        public const byte Mesh = 5;
    }

    public class PhysicsShapeParser : TypedReadParser<IPhysicsShape>
    {
        public static PhysicsShapeParser Instance = new PhysicsShapeParser();

        public PhysicsShapeParser()
        {
            Register<PhysicsBoxShape>(PhysicsShapeType.Box);
            Register<PhysicsSphereShape>(PhysicsShapeType.Sphere);
            Register<PhysicsCapsuleShape>(PhysicsShapeType.Capsule);
            Register<PhysicsCylinderShape>(PhysicsShapeType.Cylinder);
            Register<PhysicsMeshShape>(PhysicsShapeType.Mesh);
        }
    }

    public class PhysicsShapeSerializer : TypedWriteSerializer<IPhysicsShape>
    {
        public static PhysicsShapeSerializer Instance = new PhysicsShapeSerializer();

        public PhysicsShapeSerializer()
        {
            Register<PhysicsBoxShape>(PhysicsShapeType.Box);
            Register<PhysicsSphereShape>(PhysicsShapeType.Sphere);
            Register<PhysicsCapsuleShape>(PhysicsShapeType.Capsule);
            Register<PhysicsCylinderShape>(PhysicsShapeType.Cylinder);
            Register<PhysicsMeshShape>(PhysicsShapeType.Mesh);
        }
    }

    public class PhysicsBoxShape : IPhysicsShape
    {
        BoxShape _shape;

        public Shape CollisionShape
        {
            get
            {
                return _shape;
            }
        }

        public PhysicsBoxShape() : this(JVector.One)
        {
        }

        public PhysicsBoxShape(JVector size): this(new BoxShape(size))
        {
        }

        public PhysicsBoxShape(BoxShape shape)
        {
            _shape = shape;
        }

        public object Clone()
        {
            return new PhysicsBoxShape(_shape.Size);
        }

        public void Serialize(IWriter writer)
        {
            JVectorSerializer.Instance.Serialize(_shape.Size, writer);
        }

        public void Deserialize(IReader reader)
        {
            _shape.Size = JVectorParser.Instance.Parse(reader);
            _shape.UpdateShape();
        }

        public void Copy(object other)
        {
            var shape = other as PhysicsBoxShape;
            if(shape == null)
            {
                return;
            }
            _shape.Size = shape._shape.Size;
            _shape.UpdateShape();
        }

        public override bool Equals(object obj)
        {
            return Equals((IPhysicsShape)obj);
        }

        public bool Equals(IPhysicsShape obj)
        {
            return Equals((PhysicsBoxShape)obj);
        }

        public bool Equals(PhysicsBoxShape go)
        {
            if((object)go == null)
            {
                return false;
            }
            return Compare(this, go);
        }

        public static bool operator ==(PhysicsBoxShape a, PhysicsBoxShape b)
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

        public static bool operator !=(PhysicsBoxShape a, PhysicsBoxShape b)
        {
            return !(a == b);
        }

        static bool Compare(PhysicsBoxShape a, PhysicsBoxShape b)
        {
            return a._shape.Size == b._shape.Size;
        }

        public override int GetHashCode()
        {
            return _shape.Size.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[PhysicsBoxShape:{0}]", _shape.Size);
        }
    }

    public class PhysicsCapsuleShape : IPhysicsShape
    {
        CapsuleShape _shape;

        public Shape CollisionShape
        {
            get
            {
                return _shape;
            }
        }

        public PhysicsCapsuleShape() : this(0,0.5f)
        {
        }

        public PhysicsCapsuleShape(float length, float radius)
        {
            _shape = new CapsuleShape(length, length);
        }

        public object Clone()
        {
            return new PhysicsCapsuleShape(_shape.Length, _shape.Radius);
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(_shape.Length);
            writer.Write(_shape.Radius);
        }

        public void Deserialize(IReader reader)
        {
            _shape.Length = reader.ReadSingle();
            _shape.Radius = reader.ReadSingle();
            _shape.UpdateShape();
        }

        public void Copy(object other)
        {
            var shape = other as PhysicsCapsuleShape;
            if(shape == null)
            {
                return;
            }
            _shape.Length = shape._shape.Length;
            _shape.Radius = shape._shape.Radius;
            _shape.UpdateShape();
        }

        public override bool Equals(object obj)
        {
            return Equals((IPhysicsShape)obj);
        }

        public bool Equals(IPhysicsShape obj)
        {
            return Equals((PhysicsCapsuleShape)obj);
        }

        public bool Equals(PhysicsCapsuleShape go)
        {
            if((object)go == null)
            {
                return false;
            }
            return Compare(this, go);
        }

        public static bool operator ==(PhysicsCapsuleShape a, PhysicsCapsuleShape b)
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

        public static bool operator !=(PhysicsCapsuleShape a, PhysicsCapsuleShape b)
        {
            return !(a == b);
        }

        static bool Compare(PhysicsCapsuleShape a, PhysicsCapsuleShape b)
        {
            return a._shape.Length == b._shape.Length && a._shape.Radius == b._shape.Radius;
        }

        public override int GetHashCode()
        {
            var hash = _shape.Length.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, _shape.Radius.GetHashCode());
            return hash;
        }

        public override string ToString()
        {
            return string.Format("[PhysicsCapsuleShape:{0} {1}]", _shape.Length, _shape.Radius);
        }
    }

    public class PhysicsCylinderShape : IPhysicsShape
    {
        CylinderShape _shape;

        public Shape CollisionShape
        {
            get
            {
                return _shape;
            }
        }

        public PhysicsCylinderShape() : this(2, 0.5f)
        {
        }

        public PhysicsCylinderShape(float height, float radius)
        {            
            _shape = new CylinderShape(height, radius);
        }

        public object Clone()
        {
            return new PhysicsCylinderShape(_shape.Height, _shape.Radius);
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(_shape.Height);
            writer.Write(_shape.Radius);
        }

        public void Deserialize(IReader reader)
        {
            _shape.Height = reader.ReadSingle();
            _shape.Radius = reader.ReadSingle();
            _shape.UpdateShape();
        }

        public void Copy(object other)
        {
            var shape = other as PhysicsCylinderShape;
            if(shape == null)
            {
                return;
            }
            _shape.Height = shape._shape.Height;
            _shape.Radius = shape._shape.Radius;
            _shape.UpdateShape();
        }

        public override bool Equals(object obj)
        {
            return Equals((IPhysicsShape)obj);
        }

        public bool Equals(IPhysicsShape obj)
        {
            return Equals((PhysicsCylinderShape)obj);
        }

        public bool Equals(PhysicsCylinderShape go)
        {
            if((object)go == null)
            {
                return false;
            }
            return Compare(this, go);
        }

        public static bool operator ==(PhysicsCylinderShape a, PhysicsCylinderShape b)
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

        public static bool operator !=(PhysicsCylinderShape a, PhysicsCylinderShape b)
        {
            return !(a == b);
        }

        static bool Compare(PhysicsCylinderShape a, PhysicsCylinderShape b)
        {
            return a._shape.Height == b._shape.Height && a._shape.Radius == b._shape.Radius;
        }

        public override int GetHashCode()
        {
            var hash = _shape.Height.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, _shape.Radius.GetHashCode());
            return hash;
        }

        public override string ToString()
        {
            return string.Format("[PhysicsCylinderShape:{0} {1}]", _shape.Height, _shape.Radius);
        }
    }

    public class PhysicsMeshShape : IPhysicsShape
    {
        List<JVector> _vertices;
        List<TriangleVertexIndices> _indices;
        TriangleMeshShape _shape;

        public Shape CollisionShape
        {
            get
            {
                return _shape;
            }
        }

        public PhysicsMeshShape() : this(new List<JVector>(), new List<TriangleVertexIndices>())
        {
        }

        public PhysicsMeshShape(List<JVector> vertices, List<TriangleVertexIndices> indices)
        {
            _vertices = vertices;
            _indices = indices;
            _shape = new TriangleMeshShape(new Octree(_vertices, _indices));
        }

        public Object Clone()
        {
            return new PhysicsMeshShape(
                new List<JVector>(_vertices),
                new List<TriangleVertexIndices>(_indices));
        }

        public void Serialize(IWriter writer)
        {
            JVectorSerializer.Instance.SerializeList(_vertices, writer);
            TriangleVertexIndicesSerializer.Instance.SerializeList(_indices, writer);
        }

        public void Deserialize(IReader reader)
        {
            _vertices = reader.ReadList<JVector>(JVectorParser.Instance.Parse);
            _indices = reader.ReadList<TriangleVertexIndices>(TriangleVertexIndicesParser.Instance.Parse);
            _shape = new TriangleMeshShape(new Octree(_vertices, _indices));
        }

        public void Copy(object other)
        {
            var shape = other as PhysicsMeshShape;
            if(shape == null)
            {
                return;
            }
            _vertices = new List<JVector>(shape._vertices);
            _indices = new List<TriangleVertexIndices>(shape._indices);
            _shape = new TriangleMeshShape(new Octree(_vertices, _indices));
        }

        public override bool Equals(object obj)
        {
            return Equals((IPhysicsShape)obj);
        }

        public bool Equals(IPhysicsShape obj)
        {
            return Equals((PhysicsMeshShape)obj);
        }

        public bool Equals(PhysicsMeshShape go)
        {
            if((object)go == null)
            {
                return false;
            }
            return Compare(this, go);
        }

        public static bool operator ==(PhysicsMeshShape a, PhysicsMeshShape b)
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

        public static bool operator !=(PhysicsMeshShape a, PhysicsMeshShape b)
        {
            return !(a == b);
        }

        static bool Compare(PhysicsMeshShape a, PhysicsMeshShape b)
        {
            if(a._indices.Count != b._indices.Count)
            {
                return false;
            }
            for(var i = 0; i < a._indices.Count; i++)
            {
                var idxa = a._indices[i];
                var idxb = b._indices[i];
                if(idxa.I0 != idxb.I0 || idxa.I1 != idxb.I1 || idxa.I2 != idxb.I2)
                {
                    return false;
                }
            }
            if(a._vertices.Count != b._vertices.Count)
            {
                return false;
            }
            for(var i = 0; i < a._vertices.Count; i++)
            {
                var va = a._vertices[i];
                var vb = b._vertices[i];
                if(va != vb)
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            for(var i = 0; i < _indices.Count; i++)
            {
                var idx = _indices[i];
                hash = CryptographyUtils.HashCombine(hash, idx.I0);
                hash = CryptographyUtils.HashCombine(hash, idx.I1);
                hash = CryptographyUtils.HashCombine(hash, idx.I2);
            }
            for(var i = 0; i < _vertices.Count; i++)
            {
                hash = CryptographyUtils.HashCombine(hash, _vertices[i].GetHashCode());
            }
            return hash;
        }

        public override string ToString()
        {
            return string.Format("[PhysicsMeshShape:{0} {1}]", _vertices.Count, _indices.Count);
        }
    }


    public class PhysicsSphereShape : IPhysicsShape
    {
        SphereShape _shape;

        public Shape CollisionShape
        {
            get
            {
                return _shape;
            }
        }

        public PhysicsSphereShape() : this(0.5f)
        {
        }

        public PhysicsSphereShape(float radius)
        {
            _shape = new SphereShape(radius);
        }

        public Object Clone()
        {
            return new PhysicsSphereShape(_shape.Radius);
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(_shape.Radius);
        }

        public void Deserialize(IReader reader)
        {
            _shape.Radius = reader.ReadSingle();
            _shape.UpdateShape();
        }


        public void Copy(object other)
        {
            var shape = other as PhysicsSphereShape;
            if(shape == null)
            {
                return;
            }
            _shape.Radius = shape._shape.Radius;
            _shape.UpdateShape();
        }

        public override bool Equals(object obj)
        {
            return Equals((IPhysicsShape)obj);
        }

        public bool Equals(IPhysicsShape obj)
        {
            return Equals((PhysicsSphereShape)obj);
        }

        public bool Equals(PhysicsSphereShape go)
        {
            if((object)go == null)
            {
                return false;
            }
            return Compare(this, go);
        }

        public static bool operator ==(PhysicsSphereShape a, PhysicsSphereShape b)
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

        public static bool operator !=(PhysicsSphereShape a, PhysicsSphereShape b)
        {
            return !(a == b);
        }

        static bool Compare(PhysicsSphereShape a, PhysicsSphereShape b)
        {
            return a._shape.Radius == b._shape.Radius;
        }

        public override int GetHashCode()
        {
            return _shape.Radius.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[PhysicsMeshShape:{0}]", _shape.Radius);
        }
    }
}