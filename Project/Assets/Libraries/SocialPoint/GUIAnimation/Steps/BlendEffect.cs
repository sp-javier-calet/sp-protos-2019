using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
	public abstract class BlendEffect : Effect, IBlendeableEffect
	{
		[SerializeField]
		bool _useEaseCustom = false;
		public bool UseEaseCustom { get { return _useEaseCustom; } set { _useEaseCustom = value; }  }

		[SerializeField]
		List<Vector2> _easeCustom = new List<Vector2>()
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 1f)
		};
		public List<Vector2> EaseCustom { get { return _easeCustom; } set { _easeCustom = value; } }

		[SerializeField]
		EaseType _easeType;
		public EaseType EaseType { get { return _easeType; } set { _easeType = value; } }

		public void CopyEasing(bool useEaseCustom, List<Vector2> easeCustom, EaseType easeType)
		{
			_useEaseCustom = useEaseCustom;
			_easeCustom = new List<Vector2>(easeCustom);
			_easeType = easeType;
		}

		public override void Copy(Step other)
		{
			base.Copy(other);
			CopyEasing( ((BlendEffect)other).UseEaseCustom, ((BlendEffect)other).EaseCustom, ((BlendEffect)other).EaseType);
		}

		public override void Invert (bool invertTime)
		{
			base.Invert (invertTime);
			CustomEasingUtility.Invert(_easeCustom);
		}

		public override void OnUpdate()
		{
			if(IsEnabledInHierarchy())
			{
				float actionStartTime = GetStartTime(AnimTimeMode.Global);
				float actionEndTime = GetEndTime(AnimTimeMode.Global);
				
				float t = _animation.CurrentTime;
				float prevT = _animation.PrevTime;

				if(t >= actionStartTime && prevT < actionEndTime)
				{
					float delta = Mathf.Min(t, actionEndTime) - actionStartTime;
					float duration = actionEndTime-actionStartTime;

					float blend = GetBlendValue(delta, 0f, 1f, duration);
					
					OnBlend(blend);
				}
			}
		}

		protected float GetStartBlendValue()
		{
			return GetBlendValue(0f, 0f, 1f, 1f);
		}

		protected float GetEndBlendValue()
		{
			return GetBlendValue(1f, 0f, 1f, 1f);
		}

		float GetBlendValue(float time, float start, float deltaVal, float duration)
		{
			if(_useEaseCustom)
			{
				IEaseCustom ease = EaseManager.GetInstance().GetCustom();
				return ease.ease(time, duration, _easeCustom);
			}
			else
			{
				IEase ease = EaseManager.GetInstance().Get(EaseType);
				return ease.ease(time, start, deltaVal, duration);
			}
		}

		public abstract void OnBlend(float blend);

		public override void OnReset()
		{
			OnBlend(0f);
		}
	}
}
