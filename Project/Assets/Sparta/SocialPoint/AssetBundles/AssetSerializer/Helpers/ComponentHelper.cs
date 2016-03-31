using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using SocialPoint.Attributes;
using System.Text;
using SocialPoint.AssetSerializer.Serializers;
using SocialPoint.AssetSerializer.Exceptions;
using SocialPoint.AssetSerializer.Utils;

namespace SocialPoint.AssetSerializer.Helpers
{
    public class ComponentHelper
    {
        // Ignored Assemblies
        // 'UnityEngine' components are not meant to be reconstructed, they must be ignored.
        // 'ProBuilderCore' is a tool for mofifying geometry in the editor, not required at runtime. 
        // 'PlayMaker' is a tool for visual scripting encapsulated into a DLL, which we are not able to serialize/deserialize the components, etc.
        // This components on the modifyied game objects must be ignored.
        public static string[] AssembliesExcluded = { "UnityEngine", "ProBuilderCore", "ProBuilderCore-Unity5", "PlayMaker", "DOTweenPro" };
        public static string[] ComponentsExcluded = {};

        public static List<Component> GetComponentsFromObject (UnityEngine.Object obj, bool excludeComponents = true)
        {
            if (obj is GameObject)
            {
                GameObject gameObj = obj as GameObject;
                List<Component> components = gameObj.GetComponents<Component> ().ToList ();
                for (int i = (components.Count-1); i >= 0; i--)
                {
                    Component component = components [i];

                    if (TypeUtils.CompareToNull(component)) {
                        SerializerLogger.LogError("Missing Component. Please relink or remove it to continue.");
                        components.RemoveAt (i);
                    }
                    else if (!excludeComponents || (excludeComponents && IsExcludedComponent (component)))
                    {
                        components.RemoveAt (i);
                    }
                }
                return components;
            }
            return new List<Component> ();
        }

        /// <summary>
        /// As some components could not be removed directly because they may have a RequireComponent attribute,
        /// this fuctions returns an ordered list of the components that can be removed if the list is iterated
        /// sequentialy.
        /// </summary>
        /// <returns>The sorted removable components from object.</returns>
        /// <param name="obj">Object.</param>
        /// <param name="excludeComponents">If set to <c>true</c> exclude components.</param>
        public static List<Component> GetSortedRemovableComponentsFromObject (UnityEngine.Object obj, bool excludeComponents = true)
        {
            List<Component> components = GetComponentsFromObject(obj, excludeComponents);
            List<KeyValuePair<Component, HashSet<Type>>> componentsAndRequirements = new List<KeyValuePair<Component, HashSet<Type>>> ();

            // Obtain required components for each component
            foreach (Component comp in components) {
                Type compType = comp.GetType();
                HashSet<Type> requiredComponentTypes = new HashSet<Type>();

                foreach ( object attr in compType.GetCustomAttributes(true)) {
                    if (attr.GetType() == typeof(RequireComponent)) {

                        RequireComponent reqAttr = (attr as RequireComponent);
                        // Up to three required types can be specified in a single declaration
                        Type reqCompType = reqAttr.m_Type0;
                        if (reqCompType != null)
                            requiredComponentTypes.Add(reqCompType);

                        reqCompType = reqAttr.m_Type1;
                        if (reqCompType != null)
                            requiredComponentTypes.Add(reqCompType);

                        reqCompType = reqAttr.m_Type2;
                        if (reqCompType != null)
                            requiredComponentTypes.Add(reqCompType);
                    }
                }

                KeyValuePair<Component, HashSet<Type>> componentAndRequirements = new KeyValuePair<Component, HashSet<Type>> (comp, requiredComponentTypes);

                componentsAndRequirements.Add (componentAndRequirements);
            }

            // Sort
            // Rules:
            // - components that don't have requisites go last
            // - components with requisites go just before one requisite is found in the list and always before non requisite
            componentsAndRequirements.Sort(delegate(KeyValuePair<Component, HashSet<Type>> compA,
                                                    KeyValuePair<Component, HashSet<Type>> compB )
            {
                if (compA.Value.Count == 0 && compB.Value.Count == 0) return 0;
                else if (compA.Value.Count == 0) return 1;
                else if (compB.Value.Count == 0) return -1;
                else {
                    // Component B is a requirement of Component A. A should go first
                    if (compA.Value.Contains(compB.Key.GetType())) return -1;
                    // If is the other way around or none of them are dependant, pass A through
                    else return 1;
                }
            });

            List<Component> sortedComponents = new List<Component> ();
            foreach (KeyValuePair<Component, HashSet<Type>> comp in componentsAndRequirements)
                sortedComponents.Add(comp.Key);

            return sortedComponents;
        }

        private static bool IsExcludedComponent (Component comp)
        {
            string AssemblyName = comp.GetType ().Assembly.GetName().Name;
            return AssembliesExcluded.Contains (AssemblyName);
        }

        private static bool AreAllExcludedComponents (IEnumerable<Component> components)
        {
            foreach (Component comp in components)
            {
                if (!IsExcludedComponent (comp))
                {
                    return false;
                }
            }
            return true;
        }

        public static Dictionary<string, KeyValuePair<Type, object>> GetFieldsFromComponent (Component comp, 
                              BindingFlags flags = ( ( BindingFlags.Public | BindingFlags.Instance ) & 
                              ~( BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.GetField | BindingFlags.SetField ) ) )
        {
            Dictionary<string, KeyValuePair<Type, object>> compDic = new Dictionary<string, KeyValuePair<Type, object>> ();

            foreach ( FieldInfo fi in comp.GetType().GetFields( flags ) ) {
                if ( ( fi.Attributes & FieldAttributes.Literal ) == 0 ) {
                    object[] attributes = fi.GetCustomAttributes ( true );
                    if ( AllowFieldInfo ( attributes ) ) {
                        object val = fi.GetValue ( comp );
                        compDic.Add ( fi.Name, new KeyValuePair<Type, object>(fi.FieldType, val) );
                    }
                }
            }

            foreach ( PropertyInfo pi in comp.GetType().GetProperties( flags ) ) {
                if ( pi.GetGetMethod() == null ) {
                    object[] attributes = pi.GetCustomAttributes ( true );
                    if ( AllowFieldInfo ( attributes ) && ( pi.CanWrite && pi.CanRead ) ) {
                        compDic.Add ( pi.Name, new KeyValuePair<Type, object>(pi.PropertyType, GetPropertyValue ( comp, pi )) );
                    }
                }
            }

            return compDic;
        }
        
        public static void RemoveAllBehaviorFromList( UnityEngine.Object[] objArr, bool destroyImmediate = false )
        {
            for ( int i = 0; i < objArr.Length; i++ ) {
                RemoveAllBehaviours( objArr[i], destroyImmediate );
            }
        }

        public static void RemoveAllBehaviours (UnityEngine.Object obj, bool destroyImmediate = false)
        {
            if (!obj)
            {
                return;
            }
            
            // Destroy own components
            List<Component> components = GetSortedRemovableComponentsFromObject (obj);
            foreach (Component comp in components)
            {
                if (comp is Behaviour)
                {
                    try
                    {
                        if (destroyImmediate)
                        {
                            UnityEngine.Object.DestroyImmediate (comp, true);
                        }
                        else
                        {
                            UnityEngine.Object.Destroy (comp);
                        }
                    }
                    //Seems that is not throwed
                    catch (Exception e)
                    {
                        Debug.Log ("CATCH EXCEPTION " + e);
                    }
                }
            }

            // destroy components of childs
            if ( obj is GameObject) {
                GameObject gameObject = obj as GameObject;
                for ( int i = 0; i < gameObject.transform.childCount; i++ ) {
                    GameObject child = gameObject.transform.GetChild(i).gameObject;
                    RemoveAllBehaviours( child, destroyImmediate );
                }
            }
        }

        public static void RemoveAllComponents (UnityEngine.Object obj, bool excludeComponents = true)
        {
            if (!obj)
            {
                return;
            }

            List<Component> components = GetSortedRemovableComponentsFromObject (obj, excludeComponents);
            bool exit = false;
            int index = (components.Count - 1);
            while (!exit)
            {
                Component component = components [index];

                if (component != null)
                {
                    Debug.Log ("PRE " + component.name + " / TYPE: " + component.GetType ());

                    if (!component.GetType ().Equals (typeof(Transform)))
                    {
                        try
                        {
                            UnityEngine.Object.DestroyImmediate (component);
                        }
                        //Seems that is not throwed
                        catch (Exception e)
                        {
                            Debug.Log ("CATCH EXCEPTION " + e);
                        }
                    }

                    if (component != null)
                    {
                        Debug.Log ("POST " + component.name + " / TYPE: " + component.GetType ());
                    }
                    else
                    {
                        Debug.Log ("COMPONENT HAS BEEN DESTROYED");
                    }
                }
                else
                {
                    Debug.Log ("COMPONENT IS NULL!");
                }

                components = GetSortedRemovableComponentsFromObject (obj, excludeComponents);

                Debug.Log ("COMPONENTS: " + components.Count);

                foreach (Component c in components)
                {
                    Debug.Log ("EXIST COMPONENT " + c.name + " -> TYPE: " + c.GetType ());
                }

                index--;
                if (index < 0)
                {
                    index = (components.Count - 1);
                }

                if (components.Count == 0 || (components.Count == 1 && components [0].GetType ().Equals (typeof(Transform))) || AreAllExcludedComponents (components))
                {
                    exit = true;
                }
            }
        }

        static bool AllowFieldInfo (object[] attributes)
        {
            if (attributes == null)
            {
                return true;
            }

            foreach (object attribute in attributes)
            {
                if (attribute.GetType () == typeof(HideInInspector)
                    || attribute.GetType () == typeof(ObsoleteAttribute))
                {
                    return false;
                }
            }
            return true;
        }

        public static void DeserializeScene( JSONSceneContainer jsonSceneContainer )
        {
            UnityEngine.Profiler.BeginSample ("DeserializeScene");
#if ATTR_USING_SIMPLEJSON
            IAttrParser jsonAttrParser = new SimpleJsonAttrParser ();
#else
            IAttrParser jsonAttrParser = new LitJsonAttrParser ();
#endif
            JsonData jsonData = JsonData.Parse ( jsonSceneContainer.serializationJSONData.text, jsonAttrParser );
            for ( int i = 0; i < jsonData.Count; i++ ) {
                JsonData objData = jsonData[i];
                int gameObjectInstanceID = (int) objData["instanceID"];
                GameObject go = GetGameObjectByInstanceID( jsonSceneContainer.rootGameObjects, jsonSceneContainer.rootGameObjectIDs, gameObjectInstanceID );
                if ( go != null ) {
                    DeserializeObject( go, objData );
                }
            }
            UnityEngine.Profiler.EndSample ();
        }

        private static GameObject GetGameObjectByInstanceID( GameObject[] rootGameObjects, int[] rootGameObjectIDs, int instanceID )
        {
            for ( int i = 0; i < rootGameObjectIDs.Length; i++ ) {
                if ( rootGameObjectIDs[i] == instanceID ) {
                    return rootGameObjects[i];
                }
            }
            
            return null;
        }

        public static void DeserializeObject ( UnityEngine.Object obj, string data, IAttrParser attrParser=null )
        {
            JsonData jsonData = JsonData.Parse ( data, attrParser );
            DeserializeObject( obj, jsonData );
        }

        public static void DeserializeObject ( UnityEngine.Object obj, JsonData jsonData )
        {
            // Process obj
            BuildUnityObjectAnnotatorSingleton.BeginBuilding();

            DeserializeObjectRec( obj, jsonData );

            BuildUnityObjectAnnotatorSingleton.EndBuilding();

            BuildUnityObjectAnnotatorSingleton.LinkActions();
        }
        
        private static void DeserializeObjectRec( UnityEngine.Object obj, JsonData jsonData, bool allowAllAttributres = true )
        {
            if ( !(obj is GameObject) ) return;
            GameObject gameObj = (GameObject) obj;

            // GameObjects should be disabled until they components are fully assigned, otherwise
            // the method Awake of the MonoBehaviours will be prematurely called when the behaviour
            // is assigned to the GameObject

            bool shouldBeReactivated = gameObj.activeSelf;
            gameObj.SetActive(false);

            int gameObjectInstanceID = (int) jsonData["instanceID"];

            BuildUnityObjectAnnotatorSingleton.AddObjectAndID(gameObj, gameObjectInstanceID);

            List<string> missingComponents = new List<string> ();
            List<string> missingFields     = new List<string> ();
            List<string> missingReaders    = new List<string> ();
            List<string> missingTypes      = new List<string> ();

            // Process components
            JsonData components = jsonData ["components"];
            for ( int i = 0; i < components.Count; i++ ) {

                string name = (string) components[i]["name"];
                int instanceID = (int) components[i]["instanceID"];
                bool active = (bool) components[i]["active"];

                Type type = GetType ( name );
                
                if ( type == null ) {
                    missingComponents.Add ( name );
					continue;
                }

                // The GameObject must be disabled at this point to prevent a premature call to
                // Awake in the component

                Component comp = gameObj.AddComponent ( type );

                if ( comp != null ) {

                    // set it's active state
                    if (comp is Behaviour)
                        (comp as Behaviour).enabled = active;

                    BuildUnityObjectAnnotatorSingleton.AddObjectAndID(comp, instanceID);

                    JsonData props = components[i]["props"];
                    for ( int j = 0; j < props.Count; j++ ) {

                        // Must be list/array deserializable

                        JsonData propDef = props[j];
                        string propName = (string) propDef["name"];
                        string propTypeName = (string) propDef["type"];
                        object objValue = null;

                        AbstractPropertyReader propReader = null;
                        try {
                            propReader = ReaderSerializerFactory.GetPropertyReader (propTypeName, propDef);
                        }
                        catch (Exception e) {
                            missingTypes.Add (e.ToString());
                        }

                        if(propReader != null)
                        {
                            objValue = propReader.ReadValueObject ();

                            FieldInfo fieldInfo = type.GetField ( propName );
                            if (fieldInfo != null) {

                                if (propReader is UnityObjectReferencePropertyReader && objValue != null) {
                                    LinkActionArguments linkAction = new LinkActionArguments();
                                    linkAction.actionType = LinkActionArguments.LinkActionType.LINK_OBJECT_TO_FIELD;
                                    linkAction.instanceObject = comp;
                                    linkAction.fieldInfo = fieldInfo;
                                    linkAction.refObjectId = (int)objValue;
                                    BuildUnityObjectAnnotatorSingleton.AddLinkAction(linkAction);

                                    fieldInfo.SetValue ( comp, null );
                                }
                                else {
                                    if ( allowAllAttributres || AllowFieldInfo ( fieldInfo.GetCustomAttributes ( true ) ) ) {
                                        fieldInfo.SetValue ( comp, objValue );
                                    }
                                }
                            }
                            else {
                                PropertyInfo propInfo = type.GetProperty ( propName );
                                if ( propInfo != null ) {
                                    if ( allowAllAttributres || AllowFieldInfo ( propInfo.GetCustomAttributes ( true ) ) ) {
                                        propInfo.SetValue ( comp, objValue, null );
                                    }
                                }
                                else {
                                    missingFields.Add ( name + "." + propName );
                                }
                            }
                        }
                        else
                        {
                            missingReaders.Add(propTypeName);
                        }
                    }
                }
            }
            
            if ( missingComponents.Count > 0 || missingFields.Count > 0 || missingReaders.Count > 0 ) {
                DeserializationException excpt = new DeserializationException ();
                excpt.MissingFields = missingFields;
                excpt.MissingComponents = missingComponents;
                excpt.MissingReaders = missingReaders;
                excpt.MissingTypes = missingTypes;
                Debug.LogError(excpt.ToString());
            }

            // Process children
            JsonData childrenData = jsonData["children"];
            // index the childs by their names: (log errors if duplicated names were found)
            Dictionary<string, GameObject> childDict = null;

            for ( int i = 0; i < childrenData.Count; i++ ) {
                JsonData childData = childrenData[i];
                string childTypeName   = (string) childData["type"];

                //BACKWARDS-COMPATIBLE: if 'childID' is present, use the legacy 'usecure' deserialization method
                bool bUseLegacyChildDeserialization = childData.ContainsKey("childID");
                if ( childTypeName == "UnityEngine.GameObject" ) {

                    if (bUseLegacyChildDeserialization)
                    {
                        int childID   = (int) childData["childID"];

                        Transform t = gameObj.transform.GetChild( childID );
                        if ( t != null ) {
                            DeserializeObjectRec( t.gameObject, childData );
                        }
                    }
                    else
                    {
                        if (childDict == null)
                        {
                            childDict = GetChildDict(gameObj.transform);
                        }

                        //Find child by a guaranteed-unique name
                        string childName = (string) childData["prefab"];
                        GameObject childGo;

                        if(childDict.TryGetValue(childName, out childGo))
                        {
                            DeserializeObjectRec( childGo, childData );
                        }
                    }
                }
            }

            // TODO: This is being tested ...
            if (shouldBeReactivated)
                gameObj.SetActive(true);
        }


        private static Dictionary<string, GameObject> GetChildDict(Transform parent)
        {
            Dictionary<string, GameObject> childDict = new Dictionary<string, GameObject> ();
            StringBuilder parentPrefix = new StringBuilder(""); // for error login purposes

            foreach(Transform child in parent)
            {
                if (child != null)
                {
                    var childGo = child.gameObject;
                    if (childDict.ContainsKey(childGo.name))
                    {
                        if (parentPrefix.ToString() == "")
                        {
                            var parentTransform = parent;
                            parentPrefix.Append(parentTransform.name + " - ");
                            while(parentTransform.parent != null)
                            {
                                parentTransform = parentTransform.parent;
                                parentPrefix.Insert(0, parentTransform.name + " - ");
                            }
                        }
                        DuplicatedChildNameException excpt = new DuplicatedChildNameException(childGo.name, parentPrefix.ToString());
                        Debug.LogError(excpt.ToString());
                    }
                    else
                    {
                        childDict.Add(childGo.name, childGo);
                    }
                }
            }

            return childDict;
        }


        private static object ReadValueObject (JsonData propDef )
        {
            string propType = (string)propDef ["type"];
            AbstractPropertyReader propReader = ReaderSerializerFactory.GetPropertyReader (propType, propDef);
            if (propReader != null)
            {
                return propReader.ReadValueObject ();
            }
            else
            {
                throw new Exception ("No reader found for property of type '" + propDef ["type"] + "'");
            }     
        }
        
        public static Type GetType (string TypeName)
        {
            
            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            var type = Type.GetType (TypeName);
            
            // If it worked, then we're done here
            if (type != null)
            {
                return type;
            }            

            Debug.LogError(TypeName);

            // If we still haven't found the proper type, we can enumerate all of the 
            // loaded assemblies and see if any of them define the type
            
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies ();
            foreach (var currentAssembly in assemblies)
            {
                var referencedAssemblies = currentAssembly.GetReferencedAssemblies ();
                foreach (var assemblyName in referencedAssemblies)
                {
                    try
                    {
                        // Load the referenced assembly
                        var assembly = Assembly.Load (assemblyName);
                        if (assembly != null)
                        {
                            // See if that assembly defines the named type
                            type = assembly.GetType (TypeName);
                            if (type != null)
                            {
                                return type;
                            }
                        }
                    }
                    catch
                    {
                        Debug.LogError("Component Helper : Missing assembly for " + assemblyName);
                    }
                }
            }
            
            // The type just couldn't be found...
            return null;
        }

        public static bool IsCustomGameObject( UnityEngine.Object obj )
        {
            // Is this is a GameObject that contains custom components ?
            if (GetComponentsFromObject(obj, true).Count > 0)
                return true;

            // Is any of its children a custom game object ?
            if ( obj is GameObject ) {
                GameObject gameObject = obj as GameObject;
                for ( int i = 0; i < gameObject.transform.childCount; i++ ) {
                    GameObject child = gameObject.transform.GetChild( i ).gameObject;
                    if (IsCustomGameObject(child))
                        return true;
                }
            }

            return false;
        } 

        private static object GetPropertyValue (Component comp, PropertyInfo pi)
        {
            object value = pi.GetValue (comp, null);
            
            if (value != null)
            {
                return value;
            }

            return null;
        }
    }
}
