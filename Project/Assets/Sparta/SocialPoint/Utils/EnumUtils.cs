using System.Collections.Generic;

namespace SocialPoint.Utils
{
    public static class EnumUtils
    {
        public struct DictionaryComparer<T> : IEqualityComparer<T> where T : struct
        {
            public bool Equals(T x, T y)
            {
                return GetHashCode(x) == GetHashCode(y);
            }

            public int GetHashCode(T e)
            {
                return EnumInt32ToInt.Convert(e);
            }
        }
    }
}