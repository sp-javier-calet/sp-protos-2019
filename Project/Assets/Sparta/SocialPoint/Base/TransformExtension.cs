using UnityEngine;
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

            for(int k = 0; k < transforms.Length; k++)
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

        public static void SetPositionX(this Transform transform, float value)
        {
            var tmp = transform.position;
            tmp.x = value;
            transform.position = tmp;
        }

        public static void SetPositionY(this Transform transform, float value)
        {
            var tmp = transform.position;
            tmp.y = value;
            transform.position = tmp;
        }

        public static void SetPositionZ(this Transform transform, float value)
        {
            var tmp = transform.position;
            tmp.z = value;
            transform.position = tmp;
        }

        public static void SetLocalPositionX(this Transform transform, float value)
        {
            var tmp = transform.localPosition;
            tmp.x = value;
            transform.localPosition = tmp;
        }

        public static void SetLocalPositionY(this Transform transform, float value)
        {
            var tmp = transform.localPosition;
            tmp.y = value;
            transform.localPosition = tmp;
        }

        public static void SetLocalPositionZ(this Transform transform, float value)
        {
            var tmp = transform.localPosition;
            tmp.z = value;
            transform.localPosition = tmp;
        }

        public static void SetLocalScaleX(this Transform transform, float value)
        {
            var tmp = transform.localScale;
            tmp.x = value;
            transform.localScale = tmp;
        }

        public static void SetLocalScaleY(this Transform transform, float value)
        {
            var tmp = transform.localScale;
            tmp.y = value;
            transform.localScale = tmp;
        }

        public static void SetLocalScaleZ(this Transform transform, float value)
        {
            var tmp = transform.localScale;
            tmp.z = value;
            transform.localScale = tmp;
        }

        public static void SetRotationX(this Transform transform, float value)
        {
            var tmp = transform.rotation.eulerAngles;
            tmp.x = value;
            transform.rotation = Quaternion.Euler(tmp);
        }

        public static void SetRotationY(this Transform transform, float value)
        {
            var tmp = transform.rotation.eulerAngles;
            tmp.y = value;
            transform.rotation = Quaternion.Euler(tmp);
        }

        public static void SetRotationZ(this Transform transform, float value)
        {
            var tmp = transform.rotation.eulerAngles;
            tmp.z = value;
            transform.rotation = Quaternion.Euler(tmp);
        }

        public static void SetLocalRotationX(this Transform transform, float value)
        {
            var tmp = transform.localRotation.eulerAngles;
            tmp.x = value;
            transform.localRotation = Quaternion.Euler(tmp);
        }

        public static void SetLocalRotationY(this Transform transform, float value)
        {
            var tmp = transform.localRotation.eulerAngles;
            tmp.y = value;
            transform.localRotation = Quaternion.Euler(tmp);
        }

        public static void SetLocalRotationZ(this Transform transform, float value)
        {
            var tmp = transform.localRotation.eulerAngles;
            tmp.z = value;
            transform.localRotation = Quaternion.Euler(tmp);
        }
    }
}

