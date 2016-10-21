namespace SocialPoint.IO
{
    public static class IWriterExtensions
    {
        public delegate void WriteDelegate<T>(T value, IWriter writer);

        public static void WriteArray<T>(this IWriter writer, T[] array, WriteDelegate<T> writeDelegate)
        {
            int size = (array != null) ? array.Length : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                writeDelegate(array[i], writer);
            }
        }

        public static void WriteInt32Array(this IWriter writer, int[] array)
        {
            WriteDelegate<int> serializeDelegate = (int i, IWriter w) => { 
                w.Write(i); 
            };
            writer.WriteArray<int>(array, serializeDelegate);
        }
    }
}