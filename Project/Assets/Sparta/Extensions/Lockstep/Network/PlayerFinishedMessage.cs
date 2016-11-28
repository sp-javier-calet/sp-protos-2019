using SocialPoint.IO;
using SocialPoint.Attributes;

namespace SocialPoint.Lockstep
{
    public sealed class PlayerFinishedMessage : INetworkShareable
    {
        public Attr Result { get; private set; }

        IAttrSerializer _serializer;
        IAttrParser _parser;

        public PlayerFinishedMessage(Attr result=null)
        {
            _serializer = new JsonAttrSerializer();
            _parser = new JsonAttrParser();
            Result = result;
        }

        public void Deserialize(IReader reader)
        {
            var length = reader.ReadInt32();
            if(length == 0)
            {
                Result = null;
            }
            else
            {
                Result = _parser.Parse(reader.ReadBytes(length));
            }
        }

        public void Serialize(IWriter writer)
        {
            if(Result == null)
            {
                writer.Write(0);
            }
            else
            {
                var bytes = _serializer.Serialize(Result);
                writer.Write(bytes.Length);
                writer.Write(bytes, bytes.Length);
            }
        }
    }
}
