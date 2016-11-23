using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public static class GameObjectUtility
    {
        public static T AddComponent<T>(GameObject go, int offsetStartPos, int positionFromOffset) where T : Component
        {
            var originalComps = new List<Component>(go.GetComponents<Component>());

            var copyComps = new List<Component>();

            // Save previous comps in copy Go
            var copyGo = new GameObject();
            for(int i = offsetStartPos; i < originalComps.Count; ++i)
            {
                Component copy = copyGo.AddComponent(originalComps[i].GetType());
                copy = GetCopyOf(copy, originalComps[i]);
                copyComps.Add(copy);
            }

            // Set the new component in the right position in the copy GO
            Component newComp = copyGo.AddComponent<T>();
            copyComps.Insert(positionFromOffset, newComp);

            // Destroy all original components that were just copied
            for(int i = offsetStartPos; i < originalComps.Count; ++i)
            {
                UnityEngine.Object.DestroyImmediate(originalComps[i]);
            }

            // Copy back all the new components
            for(int i = 0; i < copyComps.Count; ++i)
            {
                go.AddComponent(copyComps[i].GetType());
            }

            // Destroy the copy GO
            UnityEngine.Object.DestroyImmediate(copyGo);

            return (T)go.GetComponents<Component>()[offsetStartPos + positionFromOffset];
        }

        public static GameObject CopyGameObject<T>(GameObject target, GameObject source) where T : Component
        {
            var originalComps = new List<Component>(source.GetComponents<Component>());
			
            for(int i = 0; i < originalComps.Count; ++i)
            {
                target.AddComponent(originalComps[i].GetType());
            }

            return target;
        }

        public static T GetCopyOf<T>(T dest, T src) where T : Component
        {
            Type type = dest.GetType();
            if(type != src.GetType())
                return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            for(int i = 0, pinfosLength = pinfos.Length; i < pinfosLength; i++)
            {
                var pinfo = pinfos[i];
                if(pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(dest, pinfo.GetValue(src, null), null);
                    }
                    catch
                    {
                    }
                    // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            for(int i = 0, finfosLength = finfos.Length; i < finfosLength; i++)
            {
                var finfo = finfos[i];
                finfo.SetValue(dest, finfo.GetValue(src));
            }
            return dest;
        }

        public static void SetLayerRecursively(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            for(int k = 0; k < gameObject.transform.childCount; k++)
            {
                Transform child = gameObject.transform.GetChild(k);
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public static Vector3 GetAccumulatedLocalPosition(Transform trans)
        {
            if(trans == null)
            {
                return Vector3.zero;
            }

            return trans.localPosition + GetAccumulatedLocalPosition(trans.parent);
        }
    }
}
