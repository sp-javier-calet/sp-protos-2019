
namespace SocialPoint.IO
{
    public interface INetworkShareable
    {
        void Deserialize(IReader reader);
        void Serialize(IWriter writer);
    }

    public class NetworkShareableParser<T> : IReadParser<T> where T: INetworkShareable, new()
    {
        public T Parse(IReader reader)
        {
            var obj = new T();
            obj.Deserialize(reader);
            return obj;
        }
    }

    public class NetworkShareableSerializer<T> : IWriteSerializer<T> where T: INetworkShareable
    {
        public void Serialize(T newObj, IWriter writer)
        {
            newObj.Serialize(writer);
        }
    }

    public static class ShareableExtensions
    {
        public static void Write<T>(this IWriter writer, T obj) where T : INetworkShareable
        {
            obj.Serialize(writer);
        }

        public static T Read<T>(this IReader reader) where T : INetworkShareable, new()
        {
            var obj = new T();
            obj.Deserialize(reader);
            return obj;
        }
    }
    
}