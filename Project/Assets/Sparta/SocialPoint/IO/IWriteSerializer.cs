
namespace SocialPoint.IO
{
    public interface IWriteSerializer<T>
    {
        /**
         * should set dirty bits with the changes between newObj and oldObj
         */
        void Compare(T newObj, T oldObj, Bitset dirty);

        /**
         * should serialize newObject but only the elements that are changed
         */
        void Serialize(T newObj, T oldObj, IWriter writer, Bitset dirty);

        /**
         * should serialize the complete newObject
         */
        void Serialize(T newObj, IWriter writer);
    }

    public static class SerializerExtensions
    {
        public static void Serialize<T>(this IWriteSerializer<T> serializer, T newObj, T oldObj, IWriter writer)
        {
            var dirty = new Bitset();
            serializer.Compare(newObj, oldObj, dirty);
            dirty.Reset();
            dirty.Write(writer);
            serializer.Serialize(newObj, oldObj, writer, dirty);
        }
    }

    /**
     * this serializer is for objects that are not persistent
     * for example actions or events
     */
    public abstract class SimpleWriteSerializer<T> : IWriteSerializer<T>
    {
        public abstract void Serialize(T newObj, IWriter writer);

        public void Compare(T newObj, T oldObj, Bitset dirty)
        {
        }

        public void Serialize(T newObj, T oldObj, IWriter writer, Bitset dirty)
        {
            Serialize(newObj, writer);
        }
    }
}
