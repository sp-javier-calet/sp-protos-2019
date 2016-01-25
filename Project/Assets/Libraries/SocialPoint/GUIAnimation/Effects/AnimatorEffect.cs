using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
	[System.Serializable]
	public class AnimatorEffect : TriggerEffect 
	{
		const string kOnAnimationTriggeredMessage = "OnAnimationTriggered";

		[SerializeField]
		[ShowInEditor]
		string _stateName = "Main";
		public string StateName { get { return _stateName; } set{_stateName = value; } }

		public override void Copy (Step other)
		{
			base.Copy(other);
			CopyActionValues((AnimatorEffect) other);
		}

		public override void CopyActionValues (Effect other)
		{
			Target = other.Target;
			StateName = ((AnimatorEffect)other).StateName;
		}

		public override void OnRemoved () { }
		public override void SetOrCreateDefaultValues()
		{}

		public override void DoAction()
		{
			if(!Application.isPlaying)
			{
				return;
			}

			PlayAnimation(Target.gameObject);
		}

		void PlayAnimation(GameObject go)
		{
			Animator animator = GUIAnimationUtility.GetComponentRecursiveDown<Animator>(go);
			if(animator != null)
			{
				if(!animator.gameObject.activeSelf)
				{
					animator.gameObject.SetActive(true);
				}

				animator.Play(_stateName);
			}
		}

		public override void SaveValuesAt (float localTimeNormalized)
		{
			UnityEngine.Debug.LogWarning( GetType().ToString() + " -> SaveValues. Nothing to save :(");
		}
	}
}
