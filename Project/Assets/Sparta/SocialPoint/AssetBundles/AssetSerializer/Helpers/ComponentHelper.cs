using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SocialPoint.AssetSerializer.Exceptions;
using SocialPoint.AssetSerializer.Serializers;
using SocialPoint.AssetSerializer.Utils;
using SocialPoint.AssetSerializer.Utils.JsonSerialization;
using SocialPoint.Attributes;
using UnityEngine;

namespace SocialPoint.AssetSerializer.Helpers
{
    public static class ComponentHelper
    {
        // Ignored Assemblies
        // 'UnityEngine' components are not meant to be reconstructed, they must be ignored.
        // 'ProBuilderCore' is a tool for mofifying geometry in the editor, not required at runtime.
        // 'PlayMaker' is a tool for visual scripting encapsulated into a DLL, which we are not able to serialize/deserialize the components, etc.
        // This components on the modifyied game objects must be ignored.
        public static string[] AssembliesExcluded = {
            "UnityEngine",
            "ProBuilderCore",
            "ProBuilderCore-Unity5",
            "PlayMaker",
            "DOTweenPro"
        };
        public static string[] ComponentsExcluded = { };

        public static List<Component> GetComponentsFromObject(UnityEngine.Object obj, bool excludeComponents = true)
        {
            var gameObject = obj as GameObject;
            if(gameObject != null)
            {
                GameObject gameObj = gameObject;
                List<Component> components = gameObj.GetComponents<Component>().ToList();
                for(int i = (components.Count - 1); i >= 0; i--)
                {
                    Component component = components[i];

                    if(TypeUtils.CompareToNull(component))
                    {
                        SerializerLogger.LogError("Missing Component. Please relink or remove it to continue.");
                        components.RemoveAt(i);
                    }
                    else if(!excludeComponents || (excludeComponents && IsExcludedComponent(component)))
                    {
                        components.RemoveAt(i);
                    }
                }
                return components;
            }
            return new List<Component>();
        }

        /// <summary>
        /// As some components could not be removed directly because they may have a RequireComponent attribute,
        /// this fuctions returns an ordered list of the components that can be removed if the list is iterated
        /// sequentialy.
        /// </summary>
        /// <returns>The sorted removable components from object.</returns>
        /// <param name="obj">Object.</param>
        /// <param name="excludeComponents">If set to <c>true</c> exclude components.</param>
        public static List<Component> GetSortedRemovableComponentsFromObject(UnityEngine.Object obj, bool excludeComponents = true)
        {
            List<Component> components = GetComponentsFromObject(obj, excludeComponents);
            var componentsAndRequirements = new List<KeyValuePair<Component, HashSet<Type>>>();

            // Obtain required components for each component
            for(int i = 0, componentsCount = components.Count; i < componentsCount; i++)
            {
                Component comp = components[i];
                Type compType = comp.GetType();
                var requiredComponentTypes = new HashSet<Type>();
                for(int j = 0, maxLength = compType.GetCustomAttributes(true).Length; j < maxLength; j++)
                {
                    object attr = compType.GetCustomAttributes(true)[j];
                    var requireComponent = attr as RequireComponent;
                    if(requireComponent != null)
                    {
                        RequireComponent reqAttr = requireComponent;
                        // Up to three required types can be specified in a single declaration
                        Type reqCompType = reqAttr.m_Type0;
                        if(reqCompType != null)
                            requiredComponentTypes.Add(reqCompType);
                        reqCompType = reqAttr.m_Type1;
                        if(reqCompType != null)
                            requiredComponentTypes.Add(reqCompType);
                        reqCompType = reqAttr.m_Type2;
                        if(reqCompType != null)
                            requiredComponentTypes.Add(reqCompType);
                    }
                }
                var componentAndRequirements = new KeyValuePair<Component, HashSet<Type>>(comp, requiredComponentTypes);
                componentsAndRequirements.Add(componentAndRequirements);
            }

            // Sort
            // Rules:
            // - components that don't have requisites go last
            // - components with requisites go just before one requisite is found in the list and always before non requisite
            componentsAndRequirements.Sort(delegate(KeyValuePair<Component, HashSet<Type>> compA,
                                                    KeyValuePair<Component, HashSet<Type>> compB) {
                if(compA.Value.Count == 0 && compB.Value.Count == 0)
                    return 0;
                if(compA.Value.Count == 0)
                    return 1;
                if(compB.Value.Count == 0)
                    return -1;
                // Component B is a requirement of Component A. A should go first
                return compA.Value.Contains(compB.Key.GetType()) ? -1 : 1;
            });

            var sortedComponents = new List<Component>();
            for(int i = 0, componentsAndRequirementsCount = componentsAndRequirements.Count; i < componentsAndRequirementsCount; i++)
            {
                KeyValuePair<Component, HashSet<Type>> comp = componentsAndRequirements[i];
                sortedComponents.Add(comp.Key);
            }

            return sortedComponents;
        }

        static bool IsExcludedComponent(Component comp)
        {
            string AssemblyName = comp.GetType().Assembly.GetName().Name;
            return AssembliesExcluded.Contains(AssemblyName);
        }

        static bool AreAllExcludedComponents(IEnumerable<Component> components)
        {
            var itr = components.GetEnumerator();
            while(itr.MoveNext())
            {
                var comp = itr.Current;
                if(!IsExcludedComponent(comp))
                {
                    itr.Dispose();
                    return false;
                }
            }
            itr.Dispose();
            return true;
        }

        public static Dictionary<string, KeyValuePair<Type, object>> GetFieldsFromComponent(Component comp, 
                                                                                            BindingFlags flags = ((BindingFlags.Public | BindingFlags.Instance) &
                                                                                            ~(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.GetField | BindingFlags.SetField)))
        {
            var compDic = new Dictionary<string, KeyValuePair<Type, object>>();

            var fieldInfo = comp.GetType().GetFields(flags);
            for(int i = 0, maxLength = fieldInfo.Length; i < maxLength; i++)
            {
                FieldInfo fi = fieldInfo[i];
                if((fi.Attributes & FieldAttributes.Literal) == 0)
                {
                    object[] attributes = fi.GetCustomAttributes(true);
                    if(AllowFieldInfo(attributes))
                    {
                        object val = fi.GetValue(comp);
                        compDic.Add(fi.Name, new KeyValuePair<Type, object>(fi.FieldType, val));
                    }
                }
            }

            var propertyInfo = comp.GetType().GetProperties(flags);
            for(int i = 0, maxLength = propertyInfo.Length; i < maxLength; i++)
            {
                PropertyInfo pi = propertyInfo[i];
                if(pi.GetGetMethod() == null)
                {
                    object[] attributes = pi.GetCustomAttributes(true);
                    if(AllowFieldInfo(attributes) && (pi.CanWrite && pi.CanRead))
                    {
                        compDic.Add(pi.Name, new KeyValuePair<Type, object>(pi.PropertyType, GetPropertyValue(comp, pi)));
                    }
                }
            }

            return compDic;
        }

        public static void RemoveAllBehaviorFromList(UnityEngine.Object[] objArr, bool destroyImmediate = false)
        {
            for(int i = 0; i < objArr.Length; i++)
            {
                RemoveAllBehaviours(objArr[i], destroyImmediate);
            }
        }

        public static void RemoveAllBehaviours(UnityEngine.Object obj, bool destroyImmediate = false)
        {
            if(!obj)
            {
                return;
            }
            
            // Destroy own components
            List<Component> components = GetSortedRemovableComponentsFromObject(obj);
            for(int i = 0, componentsCount = components.Count; i < componentsCount; i++)
            {
                Component comp = components[i];
                if(comp is Behaviour)
                {
                    try
                    {
                        if(destroyImmediate)
                        {
                            UnityEngine.Object.DestroyImmediate(comp, true);
                        }
                        else
                        {
                            UnityEngine.Object.Destroy(comp);
                        }
                    }
                    //Seems that is not throwed
                    catch(Exception e)
                    {
                        Debug.Log("CATCH EXCEPTION " + e);
                    }
                }
            }

            // destroy components of childs
            var gameObject = obj as GameObject;
            if(gameObject != null)
            {
                for(int i = 0; i < gameObject.transform.childCount; i++)
                {
                    GameObject child = gameObject.transform.GetChild(i).gameObject;
                    RemoveAllBehaviours(child, destroyImmediate);
                }
            }
        }

        public static void RemoveAllComponents(UnityEngine.Object obj, bool excludeComponents = true)
        {
            if(!obj)
            {
                return;
            }

            List<Component> components = GetSortedRemovableComponentsFromObject(obj, excludeComponents);
            bool exit = false;
            int index = (components.Count - 1);
            while(!exit)
            {
                Component component = components[index];

                if(component != null)
                {
                    Debug.Log("PRE " + component.name + " / TYPE: " + component.GetType());

                    if(!component.GetType().Equals(typeof(Transform)))
                    {
                        try
                        {
                            UnityEngine.Object.DestroyImmediate(component);
                        }
                        //Seems that is not throwed
                        catch(Exception e)
                        {
                            Debug.Log("CATCH EXCEPTION " + e);
                        }
                    }

                    if(component != null)
                    {
                        Debug.Log("POST " + component.name + " / TYPE: " + component.GetType());
                    }
                    else
                    {
                        Debug.Log("COMPONENT HAS BEEN DESTROYED");
                    }
                }
                else
                {
                    Debug.Log("COMPONENT IS NULL!");
                }

                components = GetSortedRemovableComponentsFromObject(obj, excludeComponents);

                Debug.Log("COMPONENTS: " + components.Count);

                for(int i = 0, componentsCount = components.Count; i < componentsCount; i++)
                {
                    Component c = components[i];
                    Debug.Log("EXIST COMPONENT " + c.name + " -> TYPE: " + c.GetType());
                }

                index--;
                if(index < 0)
                {
                    index = (components.Count - 1);
                }

                exit |= components.Count == 0 || (components.Count == 1 && components[0].GetType().Equals(typeof(Transform))) || AreAllExcludedComponents(components);
            }
        }

        static bool AllowFieldInfo(object[] attributes)
        {
            if(attributes == null)
            {
                return true;
            }

            for(int i = 0, attributesLength = attributes.Length; i < attributesLength; i++)
            {
                object attribute = attributes[i];
                if(attribute is HideInInspector || attribute is ObsoleteAttribute)
                {
                    return false;
                }
            }
            return true;
        }

        public static void DeserializeScene(JSONSceneContainer jsonSceneContainer)
        {
            Profiler.BeginSample("DeserializeScene");
#if ATTR_USING_SIMPLEJSON
            IAttrParser jsonAttrParser = new SimpleJsonAttrParser ();
#else
            IAttrParser jsonAttrParser = new LitJsonAttrParser();
#endif
            JsonData jsonData = JsonData.Parse(jsonSceneContainer.serializationJSONData.text, jsonAttrParser);
            for(int i = 0; i < jsonData.Count; i++)
            {
                JsonData objData = jsonData[i];
                int gameObjectInstanceID = (int)objData["instanceID"];
                GameObject go = GetGameObjectByInstanceID(jsonSceneContainer.rootGameObjects, jsonSceneContainer.rootGameObjectIDs, gameObjectInstanceID);
                if(go != null)
                {
                    DeserializeObject(go, objData);
                }
            }
            Profiler.EndSample();

            BuildUnityObjectAnnotatorSingleton.Clear();
        }

        static GameObject GetGameObjectByInstanceID(GameObject[] rootGameObjects, int[] rootGameObjectIDs, int instanceID)
        {
            for(int i = 0; i < rootGameObjectIDs.Length; i++)
            {
                if(rootGameObjectIDs[i] == instanceID)
                {
                    return rootGameObjects[i];
                }
            }
            
            return null;
        }

        public static void DeserializeObject(UnityEngine.Object obj, string data, IAttrParser attrParser = null)
        {
            JsonData jsonData = JsonData.Parse(data, attrParser);
            DeserializeObject(obj, jsonData);
        }

        public static void DeserializeObject(UnityEngine.Object obj, JsonData jsonData)
        {
            // Process obj
            BuildUnityObjectAnnotatorSingleton.BeginBuilding();

            DeserializeObjectRec(obj, jsonData);

            BuildUnityObjectAnnotatorSingleton.EndBuilding();

            BuildUnityObjectAnnotatorSingleton.LinkActions();
        }

        static void DeserializeObjectRec(UnityEngine.Object obj, JsonData jsonData, bool allowAllAttributres = true)
        {
            var gameObject = obj as GameObject;
            if(gameObject == null)
                return;
            var gameObj = gameObject;

            // GameObjects should be disabled until they components are fully assigned, otherwise
            // the method Awake of the MonoBehaviours will be prematurely called when the behaviour
            // is assigned to the GameObject

            bool shouldBeReactivated = gameObj.activeSelf;
            gameObj.SetActive(false);

            int gameObjectInstanceID = (int)jsonData["instanceID"];

            BuildUnityObjectAnnotatorSingleton.AddObjectAndID(gameObj, gameObjectInstanceID);

            var missingComponents = new List<string>();
            var missingFields = new List<string>();
            var missingReaders = new List<string>();
            var missingTypes = new List<string>();

            // Process components
            JsonData components = jsonData["components"];
            for(int i = 0; i < components.Count; i++)
            {

                string name = (string)components[i]["name"];
                int instanceID = (int)components[i]["instanceID"];
                bool active = (bool)components[i]["active"];

                Type type = GetType(name);
                
                if(type == null)
                {
                    missingComponents.Add(name);
                    continue;
                }

                // The GameObject must be disabled at this point to prevent a premature call to
                // Awake in the component

                Component comp = gameObj.AddComponent(type);

                if(comp != null)
                {

                    // set it's active state
                    var behaviour = comp as Behaviour;
                    if(behaviour != null)
                        behaviour.enabled = active;

                    BuildUnityObjectAnnotatorSingleton.AddObjectAndID(comp, instanceID);

                    JsonData props = components[i]["props"];
                    for(int j = 0; j < props.Count; j++)
                    {

                        // Must be list/array deserializable

                        JsonData propDef = props[j];
                        string propName = (string)propDef["name"];
                        string propTypeName = (string)propDef["type"];
                        object objValue;

                        AbstractPropertyReader propReader = null;
                        try
                        {
                            propReader = ReaderSerializerFactory.GetPropertyReader(propTypeName, propDef);
                        }
                        catch(Exception e)
                        {
                            missingTypes.Add(e.ToString());
                        }

                        if(propReader != null)
                        {
                            objValue = propReader.ReadValueObject();

                            FieldInfo fieldInfo = type.GetField(propName);
                            if(fieldInfo != null)
                            {

                                if(propReader is UnityObjectReferencePropertyReader && objValue != null)
                                {
                                    var linkAction = new LinkActionArguments();
                                    linkAction.actionType = LinkActionArguments.LinkActionType.LINK_OBJECT_TO_FIELD;
                                    linkAction.instanceObject = comp;
                                    linkAction.fieldInfo = fieldInfo;
                                    linkAction.refObjectId = (int)objValue;
                                    BuildUnityObjectAnnotatorSingleton.AddLinkAction(linkAction);

                                    fieldInfo.SetValue(comp, null);
                                }
                                else
                                {
                                    if(allowAllAttributres || AllowFieldInfo(fieldInfo.GetCustomAttributes(true)))
                                    {
                                        fieldInfo.SetValue(comp, objValue);
                                    }
                                }
                            }
                            else
                            {
                                PropertyInfo propInfo = type.GetProperty(propName);
                                if(propInfo != null)
                                {
                                    if(allowAllAttributres || AllowFieldInfo(propInfo.GetCustomAttributes(true)))
                                    {
                                        propInfo.SetValue(comp, objValue, null);
                                    }
                                }
                                else
                                {
                                    missingFields.Add(name + "." + propName);
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
            
            if(missingComponents.Count > 0 || missingFields.Count > 0 || missingReaders.Count > 0)
            {
                var excpt = new DeserializationException();
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

            for(int i = 0; i < childrenData.Count; i++)
            {
                JsonData childData = childrenData[i];
                string childTypeName = (string)childData["type"];

                //BACKWARDS-COMPATIBLE: if 'childID' is present, use the legacy 'usecure' deserialization method
                bool bUseLegacyChildDeserialization = childData.ContainsKey("childID");
                if(childTypeName == "UnityEngine.GameObject")
                {

                    if(bUseLegacyChildDeserialization)
                    {
                        int childID = (int)childData["childID"];

                        Transform t = gameObj.transform.GetChild(childID);
                        if(t != null)
                        {
                            DeserializeObjectRec(t.gameObject, childData);
                        }
                    }
                    else
                    {
                        if(childDict == null)
                        {
                            childDict = GetChildDict(gameObj.transform);
                        }

                        //Find child by a guaranteed-unique name
                        string childName = (string)childData["prefab"];
                        GameObject childGo;

                        if(childDict.TryGetValue(childName, out childGo))
                        {
                            DeserializeObjectRec(childGo, childData);
                        }
                    }
                }
            }

            // TODO: This is being tested ...
            if(shouldBeReactivated)
                gameObj.SetActive(true);
        }


        static Dictionary<string, GameObject> GetChildDict(Transform parent)
        {
            var childDict = new Dictionary<string, GameObject>();
            var parentPrefix = new StringBuilder(""); // for error login purposes

            var itr = parent.GetEnumerator();
            while(itr.MoveNext())
            {
                var child = (Transform)itr.Current;
                if(child != null)
                {
                    var childGo = child.gameObject;
                    if(childDict.ContainsKey(childGo.name))
                    {
                        if(parentPrefix.ToString() == "")
                        {
                            var parentTransform = parent;
                            parentPrefix.Append(parentTransform.name + " - ");
                            while(parentTransform.parent != null)
                            {
                                parentTransform = parentTransform.parent;
                                parentPrefix.Insert(0, parentTransform.name + " - ");
                            }
                        }
                        var excpt = new DuplicatedChildNameException(childGo.name, parentPrefix.ToString());
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


        static object ReadValueObject(JsonData propDef)
        {
            string propType = (string)propDef["type"];
            AbstractPropertyReader propReader = ReaderSerializerFactory.GetPropertyReader(propType, propDef);
            if(propReader != null)
            {
                return propReader.ReadValueObject();
            }
            throw new Exception("No reader found for property of type '" + propDef["type"] + "'");
        }

        public static Type GetType(string TypeName)
        {
            
            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            var type = Type.GetType(TypeName);
            
            // If it worked, then we're done here
            if(type != null)
            {
                return type;
            }            

            Debug.LogError(TypeName);

            // If we still haven't found the proper type, we can enumerate all of the 
            // loaded assemblies and see if any of them define the type
            
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for(int i = 0, assembliesLength = assemblies.Length; i < assembliesLength; i++)
            {
                var currentAssembly = assemblies[i];
                var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
                for(int j = 0, referencedAssembliesLength = referencedAssemblies.Length; j < referencedAssembliesLength; j++)
                {
                    var assemblyName = referencedAssemblies[j];
                    try
                    {
                        // Load the referenced assembly
                        var assembly = Assembly.Load(assemblyName);
                        if(assembly != null)
                        {
                            // See if that assembly defines the named type
                            type = assembly.GetType(TypeName);
                            if(type != null)
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

        public static bool IsCustomGameObject(UnityEngine.Object obj)
        {
            // Is this is a GameObject that contains custom components ?
            if(GetComponentsFromObject(obj).Count > 0)
                return true;

            // Is any of its children a custom game object ?
            var gameObject = obj as GameObject;
            if(gameObject != null)
            {
                for(int i = 0; i < gameObject.transform.childCount; i++)
                {
                    GameObject child = gameObject.transform.GetChild(i).gameObject;
                    if(IsCustomGameObject(child))
                        return true;
                }
            }

            return false;
        }

        static object GetPropertyValue(Component comp, PropertyInfo pi)
        {
            object value = pi.GetValue(comp, null);
            
            return value;

        }
    }
}
