using System;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public static class SerializationUtils
    {
        public delegate void SerializeDelegate<T>(T value, IWriter writer);

        public delegate T ParseDelegate<T>(IReader reader);

        public static void SerializeArray<T>(T[] array, SerializeDelegate<T> serializeDelegate, IWriter writer)
        {
            int size = (array != null) ? array.Length : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                serializeDelegate(array[i], writer);
            }
        }

        public static T[] ParseArray<T>(ParseDelegate<T> parseDelegate, IReader reader)
        {
            int size = reader.ReadInt32();
            var array = new T[size];
            for(int i = 0; i < size; ++i)
            {
                array[i] = parseDelegate(reader);
            }
            return array;
        }

        public static void SerializeIntArray(int[] array, IWriter writer)
        {
            SerializeDelegate<int> serializeDelegate = (int i, IWriter w) => { 
                w.Write(i); 
            };
            SerializeArray<int>(array, serializeDelegate, writer);
        }

        public static int[] ParseIntArray(IReader reader)
        {
            ParseDelegate<int> parseDelegate = (IReader r) => { 
                return r.ReadInt32(); 
            };
            return ParseArray<int>(parseDelegate, reader);
        }
    }
}
