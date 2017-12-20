using System;
using System.Collections.Generic;
using Jitter.LinearMath;
using SocialPoint.IO;
using SocialPoint.Pooling;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public class SceneTransform : INetworkShareable, IEquatable<SceneTransform>, ICopyable, IPoolCloneable, ITagged
    {
        public TagSet Tags{ get; set; }

        public string Id{ get; set; }

        public Transform Transform{ get; set; }

        public List<Transform> Children{ get; set; }

        public void Deserialize(IReader reader)
        {
            Id = reader.ReadString();
            if(Tags == null)
            {
                Tags = new TagSet();
            }
            Tags.Deserialize(reader);
            Transform = TransformParser.Instance.Parse(reader);
            if(Children == null)
            {
                Children = new List<Transform>();
            }
            else
            {
                Children.Clear();
            }
            var size = reader.ReadInt32();
            for(int i = 0; i < size; ++i)
            {
                Children.Add(TransformParser.Instance.Parse(reader));
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Id);
            Tags.Serialize(writer);
            TransformSerializer.Instance.Serialize(Transform, writer);
            writer.Write(Children.Count);
            for(var i = 0; i < Children.Count; i++)
            {
                TransformSerializer.Instance.Serialize(Children[i], writer);
            }
        }

        public List<JVector> ChildPositions
        {
            get
            {
                if(Children == null)
                {
                    return null;
                }
                var pos = new List<JVector>();
                for(var j = 0; j < Children.Count; j++)
                {
                    pos.Add(Children[j].Position);
                }
                return pos;
            }
        }

        public object Clone(ObjectPool pool = null)
        {
            var st = new SceneTransform {
                Id = Id,
                Tags = (TagSet)Tags.Clone(),
                Transform = (Transform)Transform.Clone(pool),
                Children = new List<Transform>()
            };
            st.Children.Capacity = Children.Capacity;
            for(var i = 0; i < Children.Count; i++)
            {
                st.Children.Add((Transform)Children[i].Clone(pool));
            }
            return st;
        }

        public void Copy(object other)
        {
            var st = other as SceneTransform;
            if(st == null)
            {
                return;
            }
            Id = st.Id;
            Tags.Copy(st.Tags);
            Transform.Copy(st.Transform);
            Children.Clear();
            Children.Capacity = st.Children.Capacity;
            for(var i = 0; i < st.Children.Count; i++)
            {
                Children.Add((Transform)st.Children[i].Clone());
            }

        }

        public override bool Equals(object obj)
        {
            return Equals((SceneTransform)obj);
        }

        public bool Equals(SceneTransform path)
        {
            if((object)path == null)
            {
                return false;
            }
            return Compare(this, path);
        }

        public static bool operator ==(SceneTransform a, SceneTransform b)
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

        public static bool operator !=(SceneTransform a, SceneTransform b)
        {
            return !(a == b);
        }

        static bool Compare(SceneTransform a, SceneTransform b)
        {
            if(a.Id != b.Id)
            {
                return false;
            }
            if(a.Tags != b.Tags)
            {
                return false;
            }
            if(a.Transform != b.Transform)
            {
                return false;
            }
            if(a.Children.Count != b.Children.Count)
            {
                return false;
            }
            for(var i = 0; i < a.Children.Count; i++)
            {
                if(a.Children[i] != b.Children[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            var hash = Id.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, Tags.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, Transform.GetHashCode());
            for(var i = 0; i < Children.Count; i++)
            {
                hash = CryptographyUtils.HashCombine(hash, Children[i].GetHashCode());
            }
            return hash;
        }

        public override string ToString()
        {
            return string.Format("[SceneTransform:{0}-{1} {2} {3}]", Id, Tags, Transform, Children.Count);
        }
    }
}
