using UnityEngine;

public static partial class CAUtils
{
    public static void SetActiveSafe(this GameObject target, bool value)
    {
        if(target.activeSelf != value)
            target.SetActive(value);
    }

    public static T AddIfNeededComponent<T>(this GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if(component == null)
        {
            component = target.AddComponent<T>();
        }
        return component;
    }

    public static T FindObjectOfTypeRecursiveUp<T>(this GameObject target)
    {
        T obj = target.GetComponent<T>();
        if(obj != null)
        {
            return obj;
        }
        else
        {
            if(target.transform.parent != null)
            {
                return FindObjectOfTypeRecursiveUp<T>(target.transform.parent.gameObject);
            }
            else
            {
                return default(T);
            }
        }
    }

    public static Transform FindChildRecursive(this Transform parent, string name)
    {
        if(parent.name == name)
        {
            return parent;
        }

        for (var it = parent.GetEnumerator();it.MoveNext();)
        {
            Transform result = FindChildRecursive((Transform)it.Current, name);
            if(result != null)
            {
                return result;
            }
        }
        return null;
    }
	#if NGUI
    public static GameObject InstantiatePrefabWithSize(UIWidget widget, string path, GameObject parent)
    {
        GameObject obj =  NGUITools.AddChild(parent, Resources.Load(path) as GameObject);
        if(obj == null)
        {
            SocialPoint.Base.DebugUtils.Assert(false, "InstantiatePrefabWithSize() --> Prefab with path [" + path + "] doesn't exist!!");
            return null;
        }

        if(widget != null)
        {
            UIWidget cont = obj.GetComponentInChildren<UIWidget>();
            if(cont != null)
            {
                cont.SetAnchor(widget.gameObject,0,0,0,0);
                cont.ResetAndUpdateAnchors();
                cont.SetAnchor(null,0,0,0,0);
                cont.transform.localPosition  = Vector3.zero;
            }
        }

        return obj;
    }
	#endif
}