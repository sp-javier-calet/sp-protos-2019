using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
	[System.Serializable]
	public abstract class Effect : Step
	{
		[SerializeField]
		Transform _target;
		public Transform Target { get { return _target; } set { _target = value; } }

		public override void Init (Animation animation, Step parent)
		{
			base.Init(animation, parent);

			_animation.AddAction(this);
		}

		public override void Refresh() {}

		public override void Copy(Step other)
		{
			base.Copy(other);

			_target = ((Effect) other).Target;
		}

		public abstract void CopyActionValues(Effect other);

		public virtual StepMonitor CreateTargetMonitor()
		{
			return null;
		}

		public abstract void OnUpdate();

		public abstract void SetOrCreateDefaultValues();

		public abstract void OnReset();
	}
}
