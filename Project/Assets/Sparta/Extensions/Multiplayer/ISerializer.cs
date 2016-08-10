using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public interface ISerializer<T>
    {
        void Compare(T newObj, T oldObj, DirtyBits dirty);
        void Serialize(T newObj, IWriter writer, DirtyBits dirty);
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
