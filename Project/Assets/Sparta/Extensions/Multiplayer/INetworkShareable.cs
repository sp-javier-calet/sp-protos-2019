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

    public static class NetworkShareableExtensions
    {
        public static void SendMessage(this INetworkServer server, NetworkMessageData data, INetworkShareable obj)
        {
            var msg = server.CreateMessage(data);
            obj.Serialize(msg.Writer);
            msg.Send();
        }

        public static void SendMessage(this INetworkClient client, NetworkMessageData data, INetworkShareable obj)
        {
            var msg = client.CreateMessage(data);
            obj.Serialize(msg.Writer);
            msg.Send();
        }

        public static T Read<T>(this ReceivedNetworkMessage msg) where T : INetworkShareable, new()
        {
            var obj = new T();
            obj.Deserialize(msg.Reader);
            return obj;
        }
    }
    
}