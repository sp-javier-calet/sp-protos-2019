using SocialPoint.IO;
using SocialPoint.Attributes;

namespace SocialPoint.Lockstep
{
    public sealed class AttrMessage : INetworkShareable
    {
        public Attr Data { get; private set; }

        IAttrSerializer _serializer;
        IAttrParser _parser;

        public AttrMessage(Attr result=null)
        {
            _serializer = new JsonAttrSerializer();
            _parser = new JsonAttrParser();
            Data = result;
        }

        public void Deserialize(IReader reader)
        {
            var length = reader.ReadInt32();
            if(length == 0)
            {
                Data = null;
            }
            else
            {
                Data = _parser.Parse(reader.ReadBytes(length));
            }
        }

        public void Serialize(IWriter writer)
        {
            if(Data == null)
            {
                writer.Write(0);
            }
            else
            {
                var bytes = _serializer.Serialize(Data);
                writer.Write(bytes.Length);
                writer.Write(bytes, bytes.Length);
            }
        }
    }
}
