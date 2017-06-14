using System;
using SocialPoint.IO;
using SocialPoint.Physics;
using SocialPoint.Utils;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public class SceneCollider : INetworkShareable, IEquatable<SceneCollider>, ICopyable, IPoolCloneable, ITagged
    {
        public TagSet Tags{ get; set; }

        public string Id{ get; set; }

        public Transform Transform{ get; set; }

        public IPhysicsShape Shape{ get; set; }

        public void Deserialize(IReader reader)
        {
            Id = reader.ReadString();
            if(Tags == null)
            {
                Tags = new TagSet();
            }
            Tags.Deserialize(reader);
            Transform = TransformParser.Instance.Parse(reader);
            Shape = PhysicsShapeParser.Instance.Parse(reader);
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Id);
            Tags.Serialize(writer);
            TransformSerializer.Instance.Serialize(Transform, writer);
            PhysicsShapeSerializer.Instance.Serialize(Shape, writer);
        }

        public object Clone(ObjectPool pool = null)
        {
            return new SceneCollider {
                Id = Id,
                Tags = (TagSet)Tags.Clone(),
                Transform = (Transform)Transform.Clone(pool),
                Shape = (IPhysicsShape)Shape.Clone()
            };
        }

        public void Copy(object other)
        {
            var collider = other as SceneCollider;
            if(collider == null)
            {
                return;
            }
            Id = collider.Id;
            Tags.Copy(collider.Tags);
            Transform.Copy(collider.Transform);
            Shape = (IPhysicsShape)collider.Shape.Clone();
        }

        public override bool Equals(object obj)
        {
            return Equals((SceneCollider)obj);
        }

        public bool Equals(SceneCollider collider)
        {
            if((object)collider == null)
            {
                return false;
            }
            return Compare(this, collider);
        }

        public static bool operator ==(SceneCollider a, SceneCollider b)
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

        public static bool operator !=(SceneCollider a, SceneCollider b)
        {
            return !(a == b);
        }

        static bool Compare(SceneCollider a, SceneCollider b)
        {
            return a.Id == b.Id && a.Tags == b.Tags && a.Transform == b.Transform && a.Shape == b.Shape;
        }

        public override int GetHashCode()
        {
            var hash = Id.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, Tags.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, Transform.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, Shape.GetHashCode());
            return hash;
        }

        public override string ToString()
        {
            return string.Format("[Collider:{0}-{1} {2} {3}]", Id, Tags, Transform, Shape);
        }
    }
}
