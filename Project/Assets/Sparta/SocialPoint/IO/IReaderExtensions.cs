namespace SocialPoint.IO
{
    public static class IReaderExtensions
    {
        public delegate T ReadDelegate<T>(IReader reader);

        public static T[] ReadArray<T>(this IReader reader, ReadDelegate<T> readDelegate)
        {
            int size = reader.ReadInt32();
            var array = new T[size];
            for(int i = 0; i < size; ++i)
            {
                array[i] = readDelegate(reader);
            }
            return array;
        }

        public static int[] ReadInt32Array(this IReader reader)
        {
            ReadDelegate<int> parseDelegate = (IReader r) => { 
                return r.ReadInt32(); 
            };
            return reader.ReadArray<int>(parseDelegate);
        }
    }
}