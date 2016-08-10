using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public interface ISerializer<T>
    {
        /**
         * should set dirty bits with the changes between newObj and oldObj
         */
        void Compare(T newObj, T oldObj, DirtyBits dirty);

        /**
         * should serialize newObject but only the elements that are changed
         */
        void Serialize(T newObj, IWriter writer, DirtyBits dirty);

        /**
         * should serialize the complete newObject
         */
        void Serialize(T newObj, IWriter writer);
    }

    public static class SerializerExtensions
    {
        public static void Serialize<T>(this ISerializer<T> serializer, T newObj, T oldObj, IWriter writer)
        {
            var dirty = new DirtyBits();
            serializer.Compare(newObj, oldObj, dirty);
            dirty.Reset();
            dirty.Write(writer);
            serializer.Serialize(newObj, writer, dirty);
        }
    }
}
