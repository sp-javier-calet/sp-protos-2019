using SocialPoint.AssetSerializer.Utils.JsonSerialization;


namespace SocialPoint.AssetSerializer.Serializers
{
    public class UnityObjectReferencePropertyReader : AbstractPropertyReader
    {
        public UnityObjectReferencePropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            if (value == null)
            {
                return null;
            } else {
                return (int)value;
            }
        }
    }
}

