using UnityEngine;

namespace SocialPoint.GUIAnimation
{
	public static class AnimationTransformUtility
	{
		public static RectTransform CreateRectTransform(string name)
		{
			GameObject go = new GameObject(name);
			return GetAddRectTransform(go);
		}

		public static RectTransform GetAddRectTransform(Transform trans)
		{
			return GetAddRectTransform(trans.gameObject);
		}

		public static RectTransform GetAddRectTransform(GameObject go)
		{
			RectTransform transform = go.GetComponent<RectTransform>();
			if(transform == null)
			{
				transform = go.AddComponent<RectTransform>();
			}
			
			return transform;
		}
	}
}
