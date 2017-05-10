using System.Collections.Generic;
using System;

namespace SocialPoint.IO
{
    public static class IReaderExtensions
    {
        public static T[] ReadArray<T>(this IReader reader, Func<IReader, T> readDelegate)
        {
            int size = reader.ReadInt32();
            var array = new T[size];
            for(int i = 0; i < size; ++i)
            {
                array[i] = readDelegate(reader);
            }
            return array;
        }

        public static T[] ReadArray<T>(this IReader reader) where T : INetworkShareable, new()
        {
            int size = reader.ReadInt32();
            var array = new T[size];
            for(int i = 0; i < size; ++i)
            {
                var elm = new T();
                elm.Deserialize(reader);
                array[i] = elm;
            }
            return array;
        }

        public static List<T> ReadList<T>(this IReader reader, Func<IReader, T> readDelegate)
        {
            int size = reader.ReadInt32();
            var list = new List<T>(size);
            for(int i = 0; i < size; ++i)
            {
                list.Add(readDelegate(reader));
            }
            return list;
        }

        public static List<T> ReadList<T>(this IReader reader) where T : INetworkShareable, new()
        {
            int size = reader.ReadInt32();
            var list = new List<T>(size);
            for(int i = 0; i < size; ++i)
            {
                var elm = new T();
                elm.Deserialize(reader);
                list.Add(elm);
            }
            return list;
        }

        public static List<T> ReadCompleteList<T>(this IReader reader, Func<IReader, T> readDelegate)
        {
            var list = new List<T>();
            while(!reader.Finished)
            {
                list.Add(readDelegate(reader));
            }
            return list;
        }

        public static T[] ReadCompleteArray<T>(this IReader reader, Func<IReader, T> readDelegate)
        {
            return reader.ReadCompleteList(readDelegate).ToArray();
        }

        public static byte[] ReadByteArray(this IReader reader)
        {
            return reader.ReadBytes(reader.ReadInt32());
        }

        public static byte[] ReadCompleteByteArray(this IReader reader)
        {
            return reader.ReadCompleteArray<byte>((IReader r) => { 
                return r.ReadByte(); 
            });
        }

        public static int[] ReadInt32Array(this IReader reader)
        {
            return reader.ReadArray<int>((IReader r) => { 
                return r.ReadInt32(); 
            });
        }

        public static List<int> ReadInt32List(this IReader reader)
        {
            return reader.ReadList<int>((IReader r) => { 
                return r.ReadInt32(); 
            });
        }

        public static string[] ReadStringArray(this IReader reader)
        {
            return reader.ReadArray<string>((IReader r) => { 
                return r.ReadString(); 
            });
        }

        public static List<string> ReadStringList(this IReader reader)
        {
            return reader.ReadList<string>((IReader r) => { 
                return r.ReadString(); 
            });
        }
    }
}
