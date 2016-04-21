using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class BooleanPropertyReader : AbstractPropertyReader
    {
        public BooleanPropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            return (bool)value;
        }
    }
}