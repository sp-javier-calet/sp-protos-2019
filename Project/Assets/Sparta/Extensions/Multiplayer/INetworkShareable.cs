using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public interface INetworkShareable
    {
        void Deserialize(IReader reader);
        void Serialize(IWriter writer);
    }

    public class NetworkShareableParser<T> : SimpleParser<T> where T: INetworkShareable, new()
    {
        public override T Parse(IReader reader)
        {
            var obj = new T();
            obj.Deserialize(reader);
            return obj;
        }
    }

    public class NetworkShareableSerializer<T> : SimpleSerializer<T> where T: INetworkShareable
    {
        public override void Serialize(T newObj, IWriter writer)
        {
            newObj.Serialize(writer);
        }
    }
    
}