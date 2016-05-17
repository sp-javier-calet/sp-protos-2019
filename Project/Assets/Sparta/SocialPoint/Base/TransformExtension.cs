using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace SocialPoint.Base
{
    public static class TransformExtension
    {
        const float kRadianToAngle = (180.0f / Mathf.PI);

        public static void RotateYToDir(this Transform transform, Vector3 dir, float stepSize)
        {
            float angle = transform.GetYAngleToDir(dir.normalized);

            RotateY(transform, angle * stepSize);
        }

        public static void RotateYToPoint(this Transform transform, Vector3 point, float stepSize)
        {
            Vector3 dir = point - transform.position;
            float angle = transform.GetYAngleToDir(dir.normalized);
            
            RotateY(transform, angle * stepSize);
        }

        public static void RotateY(this Transform transform, float deltaAngle)
        {
            Quaternion deltaQuad = Quaternion.AngleAxis(deltaAngle, new Vector3(0, 1, 0));
            Quaternion newQuat = transform.rotation * deltaQuad;
            transform.rotation = newQuat;
        }

        public static float GetYAngleToPoint(this Transform transform, Vector3 point)
        {
            Vector3 dir = point - transform.position;
            float angle = transform.GetYAngleToDir(dir.normalized);

            return angle;
        }

        public static float GetYAngleToDir(this Transform transform, Vector3 dir)
        {
            dir.Normalize();

            float projToFront = Vector3.Dot(dir, transform.forward.normalized);
            float projToRight = Vector3.Dot(dir, transform.right.normalized);
            
            float radians = Mathf.Atan2(projToRight, projToFront);
            float angle = kRadianToAngle * radians;
            
            return angle;
        }

        public static void ResetLocalTransform(this Transform transform)
        {
            transform.localScale = Vector3.one;
            transform.localEulerAngles = Vector3.zero;
            transform.localPosition = Vector3.zero;
        }

        public static Transform GetChildRecursive(this Transform trans, string name)
        {
            Component[] transforms = trans.GetComponentsInChildren(typeof(Transform), true);

            for(int k = 0; k < transforms.Count(); k++)
            {
                var atrans = transforms[k] as Transform;
                if(atrans.name == name)
                {
                    return atrans;
                }
            }
            return null;
        }

        public static bool IsPrefab(this Transform This)
        {
            var TempObject = new GameObject();
            try
            {
                TempObject.transform.parent = This.parent;
                
                var OriginalIndex = This.GetSiblingIndex();
                
                This.SetSiblingIndex(int.MaxValue);
                if(This.GetSiblingIndex() == 0)
                {
                    return true;
                }
                
                This.SetSiblingIndex(OriginalIndex);
                return false;
            }
            finally
            {
                Object.DestroyImmediate(TempObject);
            }
        }

        public static T SafeGetComponentInChildren<T>(this Transform parent) where T : Component
        {
            T parentComponent = parent.GetComponent<T>();

            if(parentComponent != null)
                return parentComponent;

            var itr = parent.GetEnumerator();
            while(itr.MoveNext())
            {
                var child = (Transform)itr.Current;
                T childComponent = child.SafeGetComponentInChildren<T>();

                if(childComponent != null)
                {
                    return childComponent;
                }
            }

            return null;
        }

        public static List<T> SafeGetComponentsInChildren<T>(this Transform parent) where T : Component
        {
            var componentsList = new List<T>();

            T parentComponent = parent.GetComponent<T>();

            if(parentComponent != null)
            {
                componentsList.Add(parentComponent);
            }

            var itr = parent.GetEnumerator();
            while(itr.MoveNext())
            {
                var child = (Transform)itr.Current;
                componentsList.AddRange(child.SafeGetComponentsInChildren<T>());
            }

            return componentsList;
        }
    }
}

