using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class SinglePropertyReader : AbstractPropertyReader
    {
        public SinglePropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            return float.Parse(value.ToString());
        }
    }
}