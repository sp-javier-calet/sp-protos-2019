
namespace SocialPoint.IO
{
    public interface IWriteSerializer<T>
    {
        /**
         * should serialize the complete newObject
         */
        void Serialize(T newObj, IWriter writer);
    }

    public interface IDiffWriteSerializer<T> : IWriteSerializer<T>
    {
        /**
         * should set dirty bits with the changes between newObj and oldObj
         */
        void Compare(T newObj, T oldObj, Bitset dirty);

        /**
         * should serialize newObject but only the elements that are changed
         */
        void Serialize(T newObj, T oldObj, IWriter writer, Bitset dirty);

    }

    public static class DiffWriteSerializerExtensions
    {
        public static void Serialize<T>(this IDiffWriteSerializer<T> serializer, T newObj, T oldObj, IWriter writer)
        {
            var dirty = new Bitset();
            serializer.Compare(newObj, oldObj, dirty);
            dirty.Reset();
            dirty.Write(writer);
            serializer.Serialize(newObj, oldObj, writer, dirty);
        }
    }
}