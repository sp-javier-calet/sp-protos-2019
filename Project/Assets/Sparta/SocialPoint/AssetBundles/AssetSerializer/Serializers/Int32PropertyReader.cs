using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class Int32PropertyReader : AbstractPropertyReader
    {
        public Int32PropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            return (int)value;
        }
    }
}