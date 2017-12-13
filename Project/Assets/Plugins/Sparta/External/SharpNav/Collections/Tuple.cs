using System;

namespace SharpNav
{
    public class Tuple<T1, T2>
    {
        public T1 Item1 { get; private set; }

        public T2 Item2 { get; private set; }

        internal Tuple(T1 first, T2 second)
        {
            Item1 = first;
            Item2 = second;
        }
    }

    public static class Tuple
    {
        public static Tuple<T1, T2> New<T1, T2>(T1 item1, T2 item2)
        {
            var tuple = new Tuple<T1, T2>(item1, item2);
            return tuple;
        }

        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }
    }
}

