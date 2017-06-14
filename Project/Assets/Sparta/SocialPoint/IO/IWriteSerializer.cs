using System.Collections.Generic;

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

    public static class WriteSerializerExtensions
    {
        public static void SerializeArray<T>(this IWriteSerializer<T> serializer, T[] array, IWriter writer)
        {
            int size = (array != null) ? array.Length : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                serializer.Serialize(array[i], writer);
            }
        }

        public static void SerializeList<T>(this IWriteSerializer<T> serializer, List<T> list, IWriter writer)
        {
            int size = (list != null) ? list.Count : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                serializer.Serialize(list[i], writer);
            }
        }
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
