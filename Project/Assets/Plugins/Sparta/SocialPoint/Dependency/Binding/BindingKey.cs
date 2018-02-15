using System;
using System.Collections.Generic;

namespace SocialPoint.Dependency
{
    public class BindingKeyComparer : IEqualityComparer<BindingKey>
    {
        public bool Equals(BindingKey a, BindingKey b)
        {
            return a.Type == b.Type && a.Tag == b.Tag;
        }

        public int GetHashCode(BindingKey obj)
        {
            return obj.Tag != null ? obj.Type.GetHashCode() ^ obj.Tag.GetHashCode() : obj.Type.GetHashCode();
        }
    }

    public struct BindingKey
    {
        public Type Type;
        public string Tag;

        public BindingKey(Type type, string tag)
        {
            Type = type;
            Tag = tag;
        }
    }
}