using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
	public static class GUIAnimationUtility
	{
		const string kBaseAnimationsName = "Animations";

		public static bool Play(GameObject obj, string animationName, System.Action onEndCallback = null)
		{
			SocialPoint.GUIAnimation.Animation anim = GetAnimation(obj, animationName);
			if (anim != null)
			{
				anim.Play();
				anim.OnEndCallback = onEndCallback;
			}

			if(anim == null)
			{
				TriggerCallback(onEndCallback);
			}

			return anim != null;
		}

		public static void TriggerCallback(System.Action callback)
		{
			if(callback != null)
			{
				callback();
			}
		}

		public static void StopAllAnimations(GameObject obj)
		{
			SetActiveAllAnimations(obj, true);

			Transform animationsRoot = obj.transform.Find(kBaseAnimationsName);
			if(animationsRoot == null)
			{
				return;
			}
			
			List<SocialPoint.GUIAnimation.Animation> animations = new List<SocialPoint.GUIAnimation.Animation>(animationsRoot.GetComponentsInChildren<SocialPoint.GUIAnimation.Animation>(true));
			for (int i = 0; i < animations.Count; ++i) 
			{
				animations[i].Stop();
			}
		}

		public static void Stop(GameObject obj, string animationName)
		{
			SocialPoint.GUIAnimation.Animation anim = GetAnimation(obj, animationName);
			if (anim != null)
			{
				anim.Stop();
			}
		}

		public static void Reset(GameObject obj, string animationName)
		{
			SocialPoint.GUIAnimation.Animation anim = GetAnimation(obj, animationName);
			if (anim != null)
			{
				anim.RevertToOriginal();
			}
		}

		public static void SetActiveAllAnimations(GameObject obj, bool isActive)
		{
			Transform animationsRoot = obj.transform.Find(kBaseAnimationsName);
			if(animationsRoot == null)
			{
				return;
			}

			List<Transform> children = new List<Transform>(animationsRoot.GetComponentsInChildren<Transform>(true));
			for (int i = 0; i < children.Count; ++i) 
			{
				if(children[i].gameObject.activeSelf != isActive)
				{
					children[i].gameObject.SetActive(isActive);
				}
			}
		}

		public static Animation GetAnimation(GameObject obj, string animationName)
		{
			SetActiveAllAnimations(obj, true);

			Transform t = obj.transform.Find(kBaseAnimationsName+"/"+animationName);
			if (t != null)
			{
				return t.GetComponent<Animation>();
			}

			return null;
		}

		public static T GetComponentRecursiveDown<T>(GameObject obj) where T : Component
		{
			T currObj = obj.GetComponent(typeof(T)) as T;
			if(currObj != null)
			{
				return currObj;
			}
			
			foreach (Transform child in obj.transform) 
			{
				currObj = GetComponentRecursiveDown<T>(child.gameObject);
				if(currObj != null)
				{
					break;
				}
			}
			
			return currObj;
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
	}
}
