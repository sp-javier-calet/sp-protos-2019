using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using SocialPoint.AssetSerializer.Exceptions;

namespace SocialPoint.AssetSerializer.Serializers
{
    public abstract class AbstractPropertyReader
    {
        protected string propName;
        protected string propTypeName;
        protected JsonData value;
        protected Type propType;

        public AbstractPropertyReader(JsonData propDef)
        {
            this.propType = Type.GetType((string)propDef["fullType"]);
            if (this.propType == null)
            {
                throw new MissingTypeException ((string)propDef["fullType"]);
            }
            this.propName = (string)propDef["name"];
            this.propTypeName = (string)propDef["type"];
            this.value = propDef["value"];
        }

        public abstract object ReadValueObject();
    }
}