﻿using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public interface IParser<T>
    {
        /**
         * called when the whole object has to be read
         */
        T Parse(IReader reader);

        /**
         * called when only the changes of the object have to be read
         * should read dirty bits first
         */
        T Parse(T oldObj, IReader reader, DirtyBits dirty);

        /**
         * return the amount of dirty bits the element will check
         */
        int GetDirtyBitsSize(T obj);
    }

    public static class ParserExtensions
    {
        public static T Parse<T>(this IParser<T> parser, T oldObj, IReader reader)
        {
            var dirty = new DirtyBits();
            dirty.Read(reader, parser.GetDirtyBitsSize(oldObj));
            return parser.Parse(oldObj, reader, dirty);
        }
    }

    /**
     * this parser is for objects that are not persistent
     * for example actions or events
     */
    public abstract class SimpleParser<T> : IParser<T>
    {
        public abstract T Parse(IReader reader);

        public T Parse(T oldObj, IReader reader, DirtyBits dirty)
        {
            return Parse(reader);
        }

        public int GetDirtyBitsSize(T obj)
        {
            return 0;
        }
    }
}