using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SocialPoint.Base
{
    public static class TransformExtension
    {
        private static float kRadianToAngle = (180.0f / ((float)Mathf.PI));

        public static void RotateYToDir(this Transform transform, Vector3 dir, float stepSize)
        {
            float angle = TransformExtension.GetYAngleToDir(transform, dir.normalized);

            RotateY(transform, angle * stepSize);
        }

        public static void RotateYToPoint(this Transform transform, Vector3 point, float stepSize)
        {
            Vector3 dir = point - transform.position;
            float angle = TransformExtension.GetYAngleToDir(transform, dir.normalized);
            
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
            float angle = TransformExtension.GetYAngleToDir(transform, dir.normalized);

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

        public static Transform GetChildRecursive(this Transform trans, string name)
        {
            Component[] transforms = trans.GetComponentsInChildren(typeof(Transform), true);

            for(int k = 0; k < transforms.Count(); k++)
            {
                Transform atrans = transforms[k] as Transform;
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
				if (This.GetSiblingIndex() == 0)
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
    }
}

