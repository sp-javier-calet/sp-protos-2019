using UnityEngine;
using System;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class Matrix4x4PropertyWriter : AbstractPropertyWriter
    {
        public Matrix4x4PropertyWriter(string propName, object value, Type propType) : base(propName, "Matrix4x4", value, propType)
        {
        }
        
        override public void WriteValueObject(JsonWriter writer)
        {
            Matrix4x4 matrix = (Matrix4x4)value;
            writer.WriteArrayStart();

            for(int i=0; i < 4; ++i)
                for(int j=0; j < 4; ++j)
                    writer.Write(matrix[i,j]);
            
            writer.WriteArrayEnd();
        }
    }
}