using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.IO;

/// <summary>
/// Class based in the original files from SharpNav/IO/Json.
/// </summary>
namespace SocialPoint.Pathfinding
{
    public static class SerializationUtils
    {
        public delegate void SerializeDelegate<T>(T value, IWriter writer);

        public delegate T ParseDelegate<T>(IReader reader);

        public static void SerializeArray<T>(T[] array, SerializeDelegate<T> serializeDelegate, IWriter writer)
        {
            int size = array.Length;
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


        public delegate Attr AttrSerializeDelegate<T>(T value);

        public delegate T AttrParseDelegate<T>(Attr attr);

        public static Attr Array2Attr<T>(T[] array, AttrSerializeDelegate<T> serializeDelegate)
        {
            AttrList attrList = new AttrList();
            for(int i = 0; i < array.Length; i++)
            {
                attrList.Add(serializeDelegate(array[i]));
            }
            return attrList;
        }

        public static T[] Attr2Array<T>(Attr attr, AttrParseDelegate<T> parseDelegate)
        {
            var attrList = attr.AsList;
            T[] array = new T[attrList.Count];
            for(int i = 0; i < attrList.Count; i++)
            {
                array[i] = parseDelegate(attrList[i]);
            }
            return array;
        }

        public static Attr Array2Attr(int[] array)
        {
            AttrSerializeDelegate<int> converter = (int i) => { 
                return new AttrInt(i); 
            };
            return Array2Attr<int>(array, converter);
        }

        public static int[] Attr2ArrayInt(Attr attr)
        {
            AttrParseDelegate<int> converter = (Attr a) => { 
                return a.AsValue.ToInt(); 
            };
            return Attr2Array<int>(attr, converter);
        }
    }
}
