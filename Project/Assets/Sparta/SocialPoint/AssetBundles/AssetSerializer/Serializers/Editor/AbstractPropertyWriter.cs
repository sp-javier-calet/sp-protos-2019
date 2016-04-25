using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using SocialPoint.AssetSerializer.Utils;

namespace SocialPoint.AssetSerializer.Serializers
{
    public abstract class AbstractPropertyWriter
    {
        protected string propName;
        protected string propTypeName;
        protected object value;
        protected Type propType;

        public AbstractPropertyWriter(string propName, string propTypeName, object value, Type propType)
        {
            this.propName = propName;
            this.propTypeName = propTypeName;
            this.value = value;
            this.propType = propType;
        }

        public void WriteObject(JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("name");
            writer.Write(this.propName);
            writer.WritePropertyName("type");
            writer.Write(this.propTypeName);
            writer.WritePropertyName("fullType");
            writer.Write(TypeUtils.GetSerializedType(propType));
            writer.WritePropertyName("value");
	        
            //writer.Write ((int)obj);
            this.WriteValueObject(writer);
	        
            writer.WriteObjectEnd();
        }

        public abstract void WriteValueObject(JsonWriter writer);
    }
}