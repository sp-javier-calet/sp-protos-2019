using System.Collections.Generic;
using System;
using System.Text;
using SocialPoint.IO;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace SocialPoint.Utils
{
    [Serializable]
    public class TagSet : ICloneable, ICopyable, IEquatable<TagSet>, INetworkShareable
    {
        #if UNITY_5_3_OR_NEWER
        [SerializeField]
        #endif
        List<string> _tags = new List<string>();

        public int Count
        {
            get
            {
                return _tags.Count;
            }
        }

        public TagSet(TagSet other = null)
        {
            if(other != null)
            {
                _tags.AddRange(other._tags);
            }
        }

        public bool Contains(string tag)
        {
            return _tags.Contains(tag);
        }

        public bool Add(string tag)
        {
            if(_tags.Contains(tag))
            {
                return false;
            }
            _tags.Add(tag);
            return true;
        }

        public void Add(string[] tags)
        {
            for(var i = 0; i < tags.Length; i++)
            {
                Add(tags[i]);
            }
        }

        public void Add(List<string> tags)
        {
            for(var i = 0; i < tags.Count; i++)
            {
                Add(tags[i]);
            }
        }

        public bool Remove(string tag)
        {
            return _tags.Remove(tag);
        }

        public void Clear()
        {
            _tags.Clear();
        }

        public void Set(string[] tags)
        {
            Clear();
            Add(tags);
        }

        public void Set(List<string> tags)
        {
            Clear();
            Add(tags);
        }

        public string[] ToArray()
        {
            return _tags.ToArray();
        }

        public bool MatchAny(TagSet tags)
        {
            return MatchAny(tags.ToArray());
        }

        public bool MatchAny(string[] tags)
        {
            if(tags == null || tags.Length == 0)
            {
                return _tags == null || _tags.Count == 0;
            }
            for(var i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];
                for(var j = 0; j < _tags.Count; j++)
                {
                    if(tag == _tags[j])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool MatchAll(string[] tags)
        {
            if(tags == null || tags.Length == 0)
            {
                return _tags == null || _tags.Count == 0;
            }
            for(var i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];
                var found = false;
                for(var j = 0; j < _tags.Count; j++)
                {
                    if(tag == _tags[j])
                    {
                        found = true;
                        break;
                    }
                }
                if(!found)
                {
                    return false;
                }
            }
            return true;
        }


        public object Clone()
        {
            var tags = new TagSet();
            tags.Set(_tags);
            return tags;
        }

        public void Copy(object other)
        {
            var tags = other as TagSet;
            if(tags == null)
            {
                return;
            }
            Set(tags._tags);
        }

        public void Deserialize(IReader reader)
        {
            Set(reader.ReadStringList());
        }

        public void Serialize(IWriter writer)
        {
            writer.WriteStringList(_tags);
        }

        public override bool Equals(object obj)
        {
            var go = (TagSet)obj;
            if((object)go == null)
            {
                return false;
            }
            return Compare(this, go);
        }

        public bool Equals(TagSet go)
        {
            if((object)go == null)
            {
                return false;
            }
            return Compare(this, go);
        }

        public static bool operator ==(TagSet a, TagSet b)
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

        public static bool operator !=(TagSet a, TagSet b)
        {
            return !(a == b);
        }

        static bool Compare(TagSet a, TagSet b)
        {
            if(a.Count != b.Count)
            {
                return false;
            }
            for(var i = 0; i < a._tags.Count; i++)
            {
                if(!b.Contains(a._tags[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            var hash = 0;
            for(var i = 0; i < _tags.Count; i++)
            {
                hash = hash ^ _tags[i].GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("[");
            for(var i = 0; i < _tags.Count; i++)
            {
                if(i != 0)
                {
                    builder.Append(", ");
                }
                builder.Append(_tags[i]);
            }
            builder.Append("]");
            return builder.ToString();
        }
    }

    public interface ITagged
    {
        TagSet Tags{ get; }
    }

    public static class TagSetExtensions
    {
        public static List<T> FindObjectsWithAnyTags<T>(this List<T> list, string[] tags) where T : ITagged
        {
            var objs = new List<T>();
            for(var i = 0; i < list.Count; i++)
            {
                var obj = list[i];
                if(obj.Tags.MatchAny(tags))
                {
                    objs.Add(obj);
                }
            }
            return objs;
        }

        public static T FindObjectWithAnyTags<T>(this List<T> list, string[] tags) where T : ITagged
        {
            for(var i = 0; i < list.Count; i++)
            {
                var obj = list[i];
                if(obj.Tags.MatchAny(tags))
                {
                    return obj;
                }
            }
            throw new InvalidOperationException("Could not find object with matching tags.");
        }

        public static List<T> FindObjectsWithAllTags<T>(this List<T> list, string[] tags) where T : ITagged
        {
            var objs = new List<T>();
            for(var i = 0; i < list.Count; i++)
            {
                var obj = list[i];
                if(obj.Tags.MatchAll(tags))
                {
                    objs.Add(obj);
                }
            }
            return objs;
        }

        public static T FindObjectWithAllTags<T>(this List<T> list, string[] tags) where T : ITagged
        {
            for(var i = 0; i < list.Count; i++)
            {
                var obj = list[i];
                if(obj.Tags.MatchAll(tags))
                {
                    return obj;
                }
            }
            throw new InvalidOperationException("Could not find object with matching tags.");
        }
    }
}
