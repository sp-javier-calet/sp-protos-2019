using System.Collections.Generic;

namespace SocialPoint.IO
{
    public interface IReadParser<T>
    {
        /**
         * called when the whole object has to be read
         */
        T Parse(IReader reader);
    }

    public interface IDiffReadParser<T> : IReadParser<T>
    {
        /**
         * called when only the changes of the object have to be read
         * should read dirty bits first
         */
        T Parse(T oldObj, IReader reader, Bitset dirty);

        /**
         * return the amount of dirty bits the element will check
         */
        int GetDirtyBitsSize(T obj);
    }

    public static class ReadParserExtensions
    {
        public static T[] ReadArray<T>(this IReadParser<T> parser, IReader reader)
        {
            int size = reader.ReadInt32();
            var array = new T[size];
            for(int i = 0; i < size; ++i)
            {
                array[i] = parser.Parse(reader);
            }
            return array;
        }

        public static List<T> ReadList<T>(this IReadParser<T> parser, IReader reader)
        {
            int size = reader.ReadInt32();
            var list = new List<T>(size);
            for(int i = 0; i < size; ++i)
            {
                list.Add(parser.Parse(reader));
            }
            return list;
        }

        public static List<T> ReadCompleteList<T>(this IReadParser<T> parser, IReader reader)
        {
            var list = new List<T>();
            while(!reader.Finished)
            {
                list.Add(parser.Parse(reader));
            }
            return list;
        }

        public static T[] ReadCompleteArray<T>(this IReadParser<T> parser, IReader reader)
        {
            return parser.ReadCompleteList(reader).ToArray();
        }
    }

    public static class DiffReadParserExtensions
    {
        public static T Parse<T>(this IDiffReadParser<T> parser, T oldObj, IReader reader)
        {
            var dirty = new Bitset();
            dirty.Read(reader, parser.GetDirtyBitsSize(oldObj));
            return parser.Parse(oldObj, reader, dirty);
        }
    }
}
