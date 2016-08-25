using System;
using System.Collections.Generic;
using System.Reflection;
using SocialPoint.AssetSerializer.Utils;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public sealed class WriterSerializerFactory
    {
        private static WriterSerializerFactory _instance; 
        public static WriterSerializerFactory Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new WriterSerializerFactory(); 
                }
                return _instance;
            }
        }

        private Dictionary<string, Type> _propertyWriterTypes;

        private WriterSerializerFactory()
        {
            _propertyWriterTypes = new Dictionary<string, Type>();

            // Writers
            RegisterWriterType("String",                typeof(StringPropertyWriter));
            RegisterWriterType("Boolean",               typeof(BooleanPropertyWriter));
            RegisterWriterType("Enum",                  typeof(EnumPropertyWriter));
            RegisterWriterType("Int32",                 typeof(Int32PropertyWriter));
            RegisterWriterType("Single",                typeof(SinglePropertyWriter));
            RegisterWriterType("Int64",                 typeof(Int64PropertyWriter));
            RegisterWriterType("UInt64",                typeof(UInt64PropertyWriter));
            RegisterWriterType("Double",                typeof(DoublePropertyWriter));
            RegisterWriterType("Decimal",               typeof(DecimalPropertyWriter));
            RegisterWriterType("SerializableDecimal",   typeof(SerializableDecimalPropertyWriter));
            RegisterWriterType("Vector2",               typeof(Vector2PropertyWriter));
            RegisterWriterType("Vector3",               typeof(Vector3PropertyWriter));
            RegisterWriterType("Vector4",               typeof(Vector4PropertyWriter));
            RegisterWriterType("Quaternion",            typeof(QuaternionPropertyWriter));
            RegisterWriterType("Matrix4x4",             typeof(Matrix4x4PropertyWriter));
            RegisterWriterType("Color",                 typeof(ColorPropertyWriter));
            RegisterWriterType("Color32",               typeof(Color32PropertyWriter));
            RegisterWriterType("LayerMask",             typeof(LayerMaskPropertyWriter));
            RegisterWriterType("UnityObjectReference",  typeof(UnityObjectReferencePropertyWriter));
            RegisterWriterType("List",                  typeof(ListPropertyWriter));
            RegisterWriterType("Array",                 typeof(ArrayPropertyWriter));
            RegisterWriterType("Class",                 typeof(ClassPropertyWriter));
        }

        public void RegisterWriterType(string key, Type value)
        {
            _propertyWriterTypes.Add(key, value);
        }

        Type GetWriterType(string key)
        {
            if(_propertyWriterTypes.ContainsKey(key))
            {
                return _propertyWriterTypes[key];
            }
            UnityEngine.Debug.Log("WriterSerializerFactory : not registered writer for " + key);
            return null;
        }

        // Writer
        private static Type GetPropertyWriterType( string typeName )
        {
            return Instance.GetWriterType(typeName);
        }
        
        private static AbstractPropertyWriter GetPropertyWriterInstance( string typeName, System.Object[] parameters )
        {
            Type type = GetPropertyWriterType( typeName );
            if ( type != null ) {
                return (AbstractPropertyWriter)Activator.CreateInstance( type, parameters );
            }
            
            return null;
        }
        
        public static AbstractPropertyWriter GetPropertyWriter( string propName, object obj, Type objType )
        {
            AbstractPropertyWriter propertyWriter;
            string representativeType = TypeUtils.GetRepresentativeType(objType);
            
            propertyWriter = GetPropertyWriterInstance( representativeType, new object[] { propName, obj, objType } );
            
            return propertyWriter;
        }
    }
}
