using UnityEngine;
using System;
using System.Collections.Generic;

namespace SocialPoint.Base
{
    public static class GameObjectExtension
    {
        public static T AddComponentIfNotFound<T>(this GameObject target) where T : Component
        {
            return AddComponentIfNotFound<T>(target, null);
        }

        public static T AddComponentIfNotFound<T>(this GameObject target, Action<T> configurator) where T : Component
        {
            var component = target.GetComponent<T>();
            if(component == null)
            {
                component = target.AddComponent<T>();
            }
            
            if(configurator != null)
            {
                configurator(component);
            }

            return component;
        }

        public static I GetInterfaceComponent<I>(this GameObject obj) where I : class
        {
            return obj.GetComponent(typeof(I)) as I;
        }

        public static GameObject GetParentWithInterfaceComponent<I>(this GameObject obj) where I : class
        {
            GameObject parent = obj;
            while(parent != null)
            {
                if(parent.GetInterfaceComponent<I>() != null)
                {
                    return parent;
                }
                if(parent.transform.parent == null)
                {
                    break;
                }
                parent = parent.transform.parent.gameObject;
            }
            return null;
        }

        public static I GetParentInterfaceComponent<I>(this GameObject obj) where I : class
        {
            GameObject parent = obj;
            while(parent != null)
            {
                I comp = parent.GetInterfaceComponent<I>();
                if(comp != null)
                {
                    return comp;
                }
                if(parent.transform.parent == null)
                {
                    break;
                }
                parent = parent.transform.parent.gameObject;
            }
            return null;
        }

        public static GameObject GetParentWithComponent<I>(this GameObject obj) where I : UnityEngine.Component
        {
            GameObject parent = obj;
            while(parent != null)
            {
                if(parent.GetComponent<I>() != null)
                {
                    return parent;
                }
                if(parent.transform.parent == null)
                {
                    break;
                }
                parent = parent.transform.parent.gameObject;
            }
            return null;
        }

        public static List<I> GetInterfaceComponentsInChildren<I>(this GameObject obj) where I : class
        {
            MonoBehaviour[] monoBehaviours = obj.GetComponentsInChildren<MonoBehaviour>();
            List<I> list = new List<I>();

            for(int k = 0; k < monoBehaviours.Length; k++)
            {
                MonoBehaviour behaviour = monoBehaviours.ElementAt(k);
                I component = behaviour.GetComponent(typeof(I)) as I;
                
                if(component != null)
                {
                    list.Add(component);
                }
            }
            return list;
        }

        public static List<I> FindObjectsOfInterface<I>() where I : class
        {
            MonoBehaviour[] monoBehaviours = GameObject.FindObjectsOfType<MonoBehaviour>();
            List<I> list = new List<I>();
            
            for(int k = 0; k < monoBehaviours.Length; k++)
            {
                MonoBehaviour behaviour = monoBehaviours.ElementAt(k);
                I component = behaviour.GetComponent(typeof(I)) as I;
                
                if(component != null)
                {
                    list.Add(component);
                }
            }
            return list;
        }

        public static T GetSafeComponent<T>(this GameObject obj) where T : MonoBehaviour
        {
            T component = obj.GetComponent<T>();
            
            if(component == null)
            {
                Log.e("Expected to find component of type " + typeof(T) + " but found none." + obj);
            }
            
            return component;
        }

        public static void SetRecursiveActive(this GameObject obj, bool active)
        {
            obj.SetActive(active);
            for(int k = 0; k < obj.transform.childCount; k++)
            {
                Transform tr = obj.transform.GetChild(k);
                tr.gameObject.SetRecursiveActive(active);
            }
        }

        public static void SetRecursiveColliderActive(this GameObject obj, bool active)
        {
            BoxCollider coll = obj.transform.GetComponent<BoxCollider>();
            if(coll != null)
            {
                coll.enabled = active;
            }

            for(int k = 0; k < obj.transform.childCount; k++)
            {
                Transform tr = obj.transform.GetChild(k);
                SetRecursiveColliderActive(tr.gameObject, active);
            }
        }

        public static void GetObjectsOfTypeRecursiveUp<I>(this GameObject obj, List<I> outList) where I : class
        {
            if(obj == null)
            {
                return;
            }

            I currObj = obj.GetComponent(typeof(I)) as I;
            if(currObj != null)
            {
                outList.Add(currObj);
            }

            if(obj.transform.parent != null)
            {
                GetObjectsOfTypeRecursiveUp(obj.transform.parent.gameObject, outList);
            }
        }

        public static I GetObjectOfTypeRecursiveUp<I>(this GameObject obj) where I : class
        {
            if(obj == null)
            {
                return null;
            }
            
            I currObj = obj.GetComponent(typeof(I)) as I;
            if(currObj != null)
            {
                return currObj;
            }
            
            if(obj.transform.parent != null)
            {
                return obj.transform.parent.gameObject.GetObjectOfTypeRecursiveUp<I>();
            }
            else
            {
                return currObj;
            }
        }

        public static void GetObjectsOfTypeRecursiveDown<I>(this GameObject obj, List<I> outList) where I : class
        {
            I currObj = obj.GetComponent(typeof(I)) as I;
            if(currObj != null)
            {
                outList.Add(currObj);
            }

            for(int k = 0; k < obj.transform.childCount; k++)
            {
                Transform child = obj.transform.GetChild(k);
                GetObjectsOfTypeRecursiveDown(child.gameObject, outList);
            }
        }

        public static I GetObjectOfTypeRecursiveDown<I>(this GameObject obj) where I : class
        {
            I currObj = obj.GetComponent(typeof(I)) as I;
            if(currObj != null)
            {
                return currObj;
            }

            for(int k = 0; k < obj.transform.childCount; k++)
            {
                Transform child = obj.transform.GetChild(k);
                currObj = child.gameObject.GetObjectOfTypeRecursiveDown<I>();
                if(currObj != null)
                {
                    break;
                }
            }

            return currObj;
        }

        public static GameObject GetChildRecursive(this GameObject gameObject, string name)
        {
            Component[] transforms = gameObject.transform.GetComponentsInChildren(typeof(Transform), true);

            for(int k = 0; k < transforms.Length; k++)
            {
                Transform atrans = transforms[k] as Transform;
                if(atrans.name == name)
                {
                    return atrans.gameObject;
                }
            }
            return null;
        }

        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            for(int k = 0; k < gameObject.transform.childCount; k++)
            {
                Transform child = gameObject.transform.GetChild(k);
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public static string GetFullPath(this GameObject gameObject)
        {
            string path = "/" + gameObject.name;
            while(gameObject.transform.parent != null)
            {
                path = "/" + gameObject.name + path;
                gameObject = gameObject.transform.parent.gameObject;
            }
            return path;
        }

        public static void RemoveChildren(this GameObject gameObject, bool includeInactive = true)
        {
            var children = gameObject.GetComponentsInChildren(typeof(Transform), includeInactive);

            for(int index = 0; index < children.Length; ++index)
            {
                if(children[index] != gameObject.transform)
                {
                    UnityEngine.Object.Destroy(children[index].gameObject);
                }
            }
        }

        public static void GetChildrenRecursive(this GameObject gameObject, string name, List<GameObject> outList)
        {
            if(gameObject.name.CompareTo(name) == 0)
            {
                outList.Add(gameObject);
            }

            for(int k = 0; k < gameObject.transform.childCount; k++)
            {
                GetChildrenRecursive(gameObject.transform.GetChild(k).gameObject, name, outList);
            }
        }

        public static void GetChildrenWithTagRecursive(this GameObject gameObject, string tag, List<GameObject> outList)
        {
            if(gameObject.CompareTag(tag))
            {
                outList.Add(gameObject);
            }

            for(int k = 0; k < gameObject.transform.childCount; k++)
            {
                GetChildrenWithTagRecursive(gameObject.transform.GetChild(k).gameObject, tag, outList);
            }
        }

        public static T GetChildComponent<T>(this GameObject gameObject, string name)
        {
            Transform child = gameObject.transform.Find(name);
            if(child != null)
            {
                return child.GetComponent<T>();
            }
            else
            {
                return default(T);
            }
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if(component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        public static void DestroyAnyway(this GameObject gameObject)
        {
            if(!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
            else
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }
    }
}
