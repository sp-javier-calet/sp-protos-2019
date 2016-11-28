
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