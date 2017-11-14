using System.Collections.Generic;

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

        public static void WriteArray<T>(this IWriter writer, T[] array) where T : INetworkShareable
        {
            int size = (array != null) ? array.Length : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                array[i].Serialize(writer);
            }
        }

        public static void WriteList<T>(this IWriter writer, List<T> list, WriteDelegate<T> writeDelegate)
        {
            int size = (list != null) ? list.Count : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                writeDelegate(list[i], writer);
            }
        }

        public static void WriteList<T>(this IWriter writer, List<T> list) where T : INetworkShareable
        {
            int size = (list != null) ? list.Count : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                list[i].Serialize(writer);
            }
        }

        public static void WriteByteArray(this IWriter writer, byte[] array)
        {
            writer.Write(array.Length);
            writer.Write(array, array.Length);
        }

        public static void WriteByteArray(this IWriter writer, byte[] array, int length)
        {
            writer.Write(length);
            writer.Write(array, length);
        }

        public static void WriteInt32Array(this IWriter writer, int[] array)
        {
            WriteDelegate<int> serializeDelegate = (int i, IWriter w) => { 
                w.Write(i); 
            };
            writer.WriteArray<int>(array, serializeDelegate);
        }

        public static void WriteInt32List(this IWriter writer, List<int> list)
        {
            WriteDelegate<int> serializeDelegate = (int i, IWriter w) => { 
                w.Write(i); 
            };
            writer.WriteList<int>(list, serializeDelegate);
        }

        public static void WriteStringArray(this IWriter writer, string[] array)
        {
            WriteDelegate<string> serializeDelegate = (string i, IWriter w) => { 
                w.Write(i);
            };
            writer.WriteArray<string>(array, serializeDelegate);
        }

        public static void WriteStringList(this IWriter writer, List<string> list)
        {
            WriteDelegate<string> serializeDelegate = (string i, IWriter w) => { 
                w.Write(i);
            };
            writer.WriteList<string>(list, serializeDelegate);
        }
    }
}
