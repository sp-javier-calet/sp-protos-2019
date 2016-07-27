using System;
using System.Collections.Generic;
using System.Reflection;
using SocialPoint.AssetSerializer.Utils;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;

namespace SocialPoint.AssetSerializer.Serializers
{
    public class ReaderSerializerFactory
    {
		private static ReaderSerializerFactory _instance; 
		public static ReaderSerializerFactory Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = new ReaderSerializerFactory(); 
				}
				return _instance;
			}
		}

		private Dictionary<string, Type> _propertyReaderTypes;
		private ReaderSerializerFactory()
		{
			_propertyReaderTypes = new Dictionary<string, Type>();

			//	Readers
			RegisterReaderType("String", 				typeof(StringPropertyReader));
			RegisterReaderType("Boolean", 				typeof(BooleanPropertyReader));
			RegisterReaderType("Enum", 					typeof(EnumPropertyReader));
			RegisterReaderType("Int32", 				typeof(Int32PropertyReader));
			RegisterReaderType("Single", 				typeof(SinglePropertyReader));
			RegisterReaderType("Int64", 				typeof(Int64PropertyReader));
			RegisterReaderType("UInt64", 				typeof(UInt64PropertyReader));
			RegisterReaderType("Double", 				typeof(DoublePropertyReader));
			RegisterReaderType("Decimal", 				typeof(DecimalPropertyReader));
			RegisterReaderType("SerializableDecimal", 	typeof(SerializableDecimalPropertyReader));
			RegisterReaderType("Vector2", 				typeof(Vector2PropertyReader));
			RegisterReaderType("Vector3", 				typeof(Vector3PropertyReader));
			RegisterReaderType("Vector4", 				typeof(Vector4PropertyReader));
			RegisterReaderType("Quaternion", 			typeof(QuaternionPropertyReader));
			RegisterReaderType("Matrix4x4", 			typeof(Matrix4x4PropertyReader));
			RegisterReaderType("Color", 				typeof(ColorPropertyReader));
			RegisterReaderType("Color32", 				typeof(Color32PropertyReader));
			RegisterReaderType("LayerMask", 			typeof(LayerMaskPropertyReader));
			RegisterReaderType("UnityObjectReference", 	typeof(UnityObjectReferencePropertyReader));
			RegisterReaderType("List", 					typeof(ListPropertyReader));
			RegisterReaderType("Array", 				typeof(ArrayPropertyReader));
			RegisterReaderType("Class", 				typeof(ClassPropertyReader));
		}

		public void RegisterReaderType(string key, Type value)
		{
			_propertyReaderTypes.Add(key, value);
		}

		Type GetReaderType(string key)
		{
			if(_propertyReaderTypes.ContainsKey(key))
			{
				return _propertyReaderTypes[key];
			}
            UnityEngine.Debug.Log("ReaderSerializerFactory : not registered reader for " + key);
			return null;
		}

        // Reader
		private static Type GetPropertyReaderType( string typeName )
		{
			return Instance.GetReaderType(typeName);
		}

		private static AbstractPropertyReader GetPropertyReaderInstance( string typeName, System.Object[] parameters )
		{
			Type type = GetPropertyReaderType( typeName );

			if ( type != null ) {
				return (AbstractPropertyReader)Activator.CreateInstance( type, parameters );
			}

			return null;
		}

		public static AbstractPropertyReader GetPropertyReader( string propType, JsonData propDef )
		{
            AbstractPropertyReader itemReader;

			// Prepare parameters
            string typeName = TypeUtils.TypeNameToFactoryTypeName( propType );
            System.Object[] parameters = { propDef };

            itemReader = GetPropertyReaderInstance( typeName, parameters );

            if (itemReader == null) {

                typeName = TypeUtils.TypeNameToFactoryTypeName( "Class" );
                itemReader = GetPropertyReaderInstance( typeName, parameters );
            }

            return itemReader;
		}

		public static AbstractPropertyReader GetPropertyReader( Type propType, JsonData propDef )
		{
			AbstractPropertyReader itemReader;
			string representativeType = TypeUtils.GetRepresentativeType(propType);
			
			// Prepare parameters
			System.Object[] parameters = { propDef };
			
			itemReader = GetPropertyReaderInstance( representativeType, parameters );
			
			return itemReader;
		}
    }
}
