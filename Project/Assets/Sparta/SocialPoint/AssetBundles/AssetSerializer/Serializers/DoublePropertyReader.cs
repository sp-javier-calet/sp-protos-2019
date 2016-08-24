using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class DoublePropertyReader : AbstractPropertyReader
    {
        public DoublePropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            return double.Parse(value.ToString());
        }
    }
}