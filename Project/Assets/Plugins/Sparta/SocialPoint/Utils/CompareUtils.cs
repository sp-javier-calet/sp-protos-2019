using System.Collections.Generic;

namespace SocialPoint.Utils
{
    public sealed class ReferenceComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetType().GetHashCode();
        }
    }
}
