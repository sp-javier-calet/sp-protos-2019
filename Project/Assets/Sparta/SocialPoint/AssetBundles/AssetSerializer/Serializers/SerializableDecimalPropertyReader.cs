using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using SocialPoint.AssetSerializer.Serializers;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class SerializableDecimalPropertyReader : AbstractPropertyReader
    {
        public SerializableDecimalPropertyReader(JsonData propDef) : base(propDef)
        {
        }
        
        override public object ReadValueObject()
        {
            decimal val;
            if (decimal.TryParse((string)this.value, out val))
            {
                SerializableDecimal sd = new SerializableDecimal();
                sd.value = val;

                return sd;
            } else 
                throw new Exception("SerailizableDecimal could not be deserialized");
        }
    }
}