using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public class AnimatorEffect : TriggerEffect
    {
        const string kOnAnimationTriggeredMessage = "OnAnimationTriggered";

        public override string StepName
        { 
            get
            {
                AnimationClip clip = GetCurrentAnimationClip ();
                _stepName = StepsManager.GetStepName (GetType ());
                if (clip != null)
                {
                    _stepName += " (" + clip.name + ")";
                }
                return _stepName;
            } 
            set
            {
                _stepName = value;
            }
        }

        [SerializeField]
        [ShowInEditor]
        string _stateName = "Main";

        public string StateName { get { return _stateName; } set { _stateName = value; } }

        public override void Copy (Step other)
        {
            base.Copy (other);
            CopyActionValues ((AnimatorEffect)other);
        }

        public override void CopyActionValues (Effect other)
        {
            Target = other.Target;
            StateName = ((AnimatorEffect)other).StateName;
        }

        public override void OnRemoved ()
        {
        }

        public override void SetOrCreateDefaultValues ()
        {
        }

        public override void DoAction ()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            PlayAnimation (Target.gameObject);
		
        }

        public override float GetFixedDuration ()
        {
            float duration = base.GetFixedDuration ();

            AnimationClip clip = GetCurrentAnimationClip ();
            if (clip != null)
            {
                duration = clip.length;
            }

            return duration;
        }

        AnimationClip GetCurrentAnimationClip ()
        {
            AnimationClip clip = null;

            Animator anim = GUIAnimationUtility.GetComponentRecursiveDown<Animator> (Target.gameObject);
            if (anim != null)
            {
                UnityEditor.Animations.AnimatorController ac = anim.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                for (int i = 0; i < ac.animationClips.Length; ++i)
                {
                    AnimationClip aclip = ac.animationClips [0];
                    if (_stateName == aclip.name)
                    {
                        clip = aclip;
                        break;
                    }
                }
            }

            return clip;
        }

        void PlayAnimation (GameObject go)
        {
            Animator animator = GUIAnimationUtility.GetComponentRecursiveDown<Animator> (go);
            if (animator != null)
            {
                if (!animator.gameObject.activeSelf)
                {
                    animator.gameObject.SetActive (true);
                }

                animator.Play (_stateName);
            }
        }

        public override void SaveValuesAt (float localTimeNormalized)
        {
            Debug.LogWarning (GetType () + " -> SaveValues. Nothing to save :(");
        }
    }
}
