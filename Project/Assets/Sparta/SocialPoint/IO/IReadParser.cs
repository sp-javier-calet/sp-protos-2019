
namespace SocialPoint.IO
{
    public interface IReadParser<T>
    {
        /**
         * called when the whole object has to be read
         */
        T Parse(IReader reader);

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

    public static class ParserExtensions
    {
        public static T Parse<T>(this IReadParser<T> parser, T oldObj, IReader reader)
        {
            var dirty = new Bitset();
            dirty.Read(reader, parser.GetDirtyBitsSize(oldObj));
            return parser.Parse(oldObj, reader, dirty);
        }
    }

    /**
     * this parser is for objects that are not persistent
     * for example actions or events
     */
    public abstract class SimpleReadParser<T> : IReadParser<T>
    {
        public abstract T Parse(IReader reader);

        public T Parse(T oldObj, IReader reader, Bitset dirty)
        {
            return Parse(reader);
        }

        public int GetDirtyBitsSize(T obj)
        {
            return 0;
        }
    }
}