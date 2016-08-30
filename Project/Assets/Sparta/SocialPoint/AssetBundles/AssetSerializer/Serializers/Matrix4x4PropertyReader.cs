using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class Matrix4x4PropertyReader : AbstractPropertyReader
    {
        public Matrix4x4PropertyReader(JsonData propDef) : base(propDef)
        {
        }

        override public object ReadValueObject()
        {
            Matrix4x4 matrix = new Matrix4x4();
            for(int i=0; i < 4; ++i)
                for(int j=0; j < 4; ++j)
                    matrix[i,j] = float.Parse(value[(i*4) + j].ToString());

            return matrix;
        }
    }
}
