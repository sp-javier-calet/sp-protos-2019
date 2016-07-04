using System.Collections.Generic;
using System;
using System.Collections;

namespace SocialPoint.Base
{
    public static class IEnumerableExtensions
    {
        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            var enumerator = source.GetEnumerator();
            while(enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if(predicate(current))
                {
                    enumerator.Dispose();
                    return current;
                }
            }
            enumerator.Dispose();
            return default(TSource);
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            var e1 = first.GetEnumerator();
            var e2 = second.GetEnumerator();
            bool result = true;
            var comparer = EqualityComparer<TSource>.Default;
            while(e1.MoveNext() && result)
            {
                if(!e2.MoveNext())
                {
                    result = false;
                    break;
                }
                if(!comparer.Equals(e1.Current, e2.Current))
                {
                    result = false;
                    break;
                }
            }

            result &= !e2.MoveNext();
            e1.Dispose();
            e2.Dispose();
            return result;
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            var enumerator = source.GetEnumerator();
            TSource result;
            if(enumerator.MoveNext())
            {
                result = enumerator.Current;
            }
            else
            {
                result = default(TSource);
            }
            enumerator.Dispose();
            return result;
        }

        public static List<T> Where<T>(this List<T> source, Func<T, bool> predicate)
        {
            List<T> result = new List<T>();
            for(int i = 0; i < source.Count; ++i)
            {
                var item = source[i];
                if(predicate(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public static bool Contains<T>(this T[] source, T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for(int i = 0; i < source.Length; ++i)
            {
                if(comparer.Equals(source[i], item))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<T> ToList<T>(this T[] source)
        {
            if(source == null)
            {
                return null;
            }
            List<T> result = new List<T>(source.Length);
            for(int i = 0; i < source.Length; ++i)
            {
                result.Add(source[i]);
            }
            return result;
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
        {
            var enumerator = source.GetEnumerator();
            while(enumerator.MoveNext())
            {
                if(index == 0)
                {
                    var result = enumerator.Current;
                    enumerator.Dispose();
                    return result;
                }
                index--;
            }
            enumerator.Dispose();
            throw new IndexOutOfRangeException();
        }

        public static List<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            List<TResult> result = new List<TResult>();
            var enumerator = source.GetEnumerator();
            while(enumerator.MoveNext())
            {
                result.Add(selector(enumerator.Current));
            }
            enumerator.Dispose();
            return result;
        }

        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            var enumerator = source.GetEnumerator();
            if(!enumerator.MoveNext())
            {
                throw new Exception("No elements");
            }
            TSource result = enumerator.Current;
            while(enumerator.MoveNext())
            {
                result = func(result, enumerator.Current);
            }
            enumerator.Dispose();
            return result;
        }
    }
}

