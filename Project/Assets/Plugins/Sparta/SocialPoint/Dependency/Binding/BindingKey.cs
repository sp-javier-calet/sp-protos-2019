using System;
using SocialPoint.Utils;

namespace SocialPoint.Dependency
{
    public struct BindingKey : IEquatable<BindingKey>
    {
        public readonly Type Type;
        public readonly string Tag;

        public BindingKey(Type type, string tag = null)
        {
            Type = type;
            Tag = tag;
        }

        public bool Equals(BindingKey other)
        {
            return Type.Equals(Type, other.Type) && string.Equals(Tag, other.Tag);
        }

        public override bool Equals(object obj){
            if(!(obj is BindingKey))
            {
                return false;
            }
            return Equals((BindingKey)obj);
        }

        public override int GetHashCode()
        {
            var hash = Type == null ? 0 : Type.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, Tag == null ? 0 : Tag.GetHashCode());
            return hash;
        }
    }
}