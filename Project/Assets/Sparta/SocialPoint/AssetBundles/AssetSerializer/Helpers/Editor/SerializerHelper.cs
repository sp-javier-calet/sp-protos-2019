using System;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using System.Text;
using SocialPoint.AssetSerializer.Serializers;
using SocialPoint.AssetSerializer.Exceptions;
using SocialPoint.AssetSerializer.Utils;


namespace SocialPoint.AssetSerializer.Helpers
{
    public class SerializerHelper
    {
        // Serialize a Scenes objects
        public static string SerializeScene( UnityEngine.Object[] objectArr, bool excludeComponents = true, bool serializeNonCustomGameObjects = false )
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter writer = new JsonWriter( sb );
            
            SerializeSceneToWriter( objectArr, writer, excludeComponents, serializeNonCustomGameObjects );
            
            return sb.ToString ();
        }
        
        private static void SerializeSceneToWriter( UnityEngine.Object[] objectArr, JsonWriter writer, bool excludeComponents = true, bool serializeNonCustomGameObjects = false )
        {
            // Open Main
            writer.WriteArrayStart ();
            
            for ( int i = 0; i < objectArr.Length; i++ ) {
                SerializeObjectToWriter( objectArr[i], writer, excludeComponents, serializeNonCustomGameObjects );
            }
            
            // Close main
            writer.WriteArrayEnd ();
        }
        
        /**
         * Example:
         *  {
         *    "prefab": "Sample_cube_prefab",
         *    "type": "UnityEngine.GameObject",
         *    "components": [
         *      {
         *        "name": "SampleComponentA",
         *        "props": [ ... ]
         *      }
         *    ]
         *    "children": [
         *      { "prefab":"some_name", ... },
         *      ...
         *    ]
         *  }
         */
        public static string SerializeObject( UnityEngine.Object obj, bool excludeComponents = true, bool serializeNonCustomGameObjects = false )
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter writer = new JsonWriter( sb );
            
            SerializeObjectToWriter( obj, writer, excludeComponents, serializeNonCustomGameObjects );
            
            return sb.ToString ();
        }
        
        private static void SerializeObjectToWriter( UnityEngine.Object obj, JsonWriter writer, bool excludeComponents = true, bool serializeNonCustomGameObjects = false, int childIndx = 0 )
        {
            SerializerLogger.AddCurrentGameObject(obj.name);
            
            if (!serializeNonCustomGameObjects && !ComponentHelper.IsCustomGameObject(obj)) {
                
                SerializerLogger.LogMsg ("Not serialized. Is not a custom GameObject.");
            }
            else
            {
                // Open Main
                writer.WriteObjectStart ();
                
                // Write GameObject Descriptiom
                writer.WritePropertyName ( "prefab" );
                writer.Write ( obj.name );
                writer.WritePropertyName ( "type" );
                writer.Write ( obj.GetType ().ToString () );
                if ( obj is GameObject ) { 
                    writer.WritePropertyName( "instanceID" );
                    writer.Write( obj.GetInstanceID() );
                }
                //DEPRECATED
                // Write child index if or zero for default
//                writer.WritePropertyName ( "childID" );
//                writer.Write ( childIndx );
                
                // Process components
                // Components must be written in inverse requirement order so the components without
                // RequiredComponents go first( later they will be instantiated in this same order )
                List<Component> components = ComponentHelper.GetSortedRemovableComponentsFromObject (obj, excludeComponents);
                components.Reverse();
                
                writer.WritePropertyName ( "components" );
                writer.WriteArrayStart ();
                foreach ( Component comp in components ) {
                    // Serialize to JSON only if it is a Behaviour!
                    if ( comp is Behaviour ) {
                        SerializerLogger.AddCurrentComponent(comp.GetType().Name);
                        
                        writer.WriteObjectStart ();
                        
                        // Write Component Description
                        writer.WritePropertyName ( "name" );
                        writer.Write ( comp.GetType ().FullName );
                        writer.WritePropertyName( "instanceID" );
                        writer.Write( comp.GetInstanceID() );
                        // Write if component is active
                        writer.WritePropertyName ( "active" );
                        writer.Write ( (comp as Behaviour).enabled );
                        writer.WritePropertyName ( "props" );
                        writer.WriteArrayStart ();
                        
                        // Write Properties description
                        Dictionary<string, KeyValuePair<Type, object>> compDic = ComponentHelper.GetFieldsFromComponent ( comp );
                        foreach ( KeyValuePair<string, KeyValuePair<Type, object>> entry in compDic ) {
                            if ( entry.Key != "name" ) {
                                WritePropValue ( entry.Value.Value, entry.Value.Key, writer, entry.Key, obj.name );
                            }
                        }
                        
                        writer.WriteArrayEnd ();                    
                        writer.WriteObjectEnd ();
                        
                        SerializerLogger.RemoveNode();
                    }
                }
                writer.WriteArrayEnd ();
                
                // Process Children
                writer.WritePropertyName ("children");
                writer.WriteArrayStart ();
                if ( obj is GameObject ) {

                    //Ensure unique children names
                    HashSet<string> childNamesSet = new HashSet<string> ();

                    GameObject gameObject = obj as GameObject;
                    foreach (Transform child in gameObject.transform)
                    {
                        if (child != null)
                        {
                            if (childNamesSet.Contains(child.name))
                            {
                                SerializerLogger.LogError(String.Format("duplicated child name '{0}'", child.name));
                            }
                            else
                            {
                                childNamesSet.Add(child.name);
                            }

                            SerializeObjectToWriter( child.gameObject, writer, excludeComponents, serializeNonCustomGameObjects );
                        }
                    }
                    //DEPRECATED
//                    for ( int i = 0; i < gameObject.transform.childCount; i++ ) {
//                        GameObject child = gameObject.transform.GetChild( i ).gameObject;
//                        SerializeObjectToWriter( child, writer, excludeComponents, serializeNonCustomGameObjects, i );
//                    }
                }
                writer.WriteArrayEnd ();
                
                // Close main
                writer.WriteObjectEnd ();
            }
            
            SerializerLogger.RemoveNode();
        }
        
        private static void WritePropValue (object obj, Type objType, JsonWriter writer, string propName, string prefabName)
        {
            try {
                AbstractPropertyWriter propWriter = WriterSerializerFactory.GetPropertyWriter ( propName, obj, objType );
                propWriter.WriteObject ( writer );
            } catch (SerializationTypeNotSupportedException e) {
                SerializerLogger.LogWarning( String.Format("{0}. Ignorig this property.", e.message) );
            }
        }
    }
}
