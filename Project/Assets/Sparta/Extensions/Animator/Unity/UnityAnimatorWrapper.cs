using UnityEngine;
using System;

namespace SocialPoint.Animations
{
    public class UnityAnimatorWrapperEventReceiver : MonoBehaviour
    {
        public UnityAnimatorWrapper AnimatorWrapper;

        void OnAnimationEvent(AnimationEvent ev)
        {
            if(AnimatorWrapper != null)
            {
                AnimatorWrapper.OnAnimationEvent(ev);
            }
        }

        void OnAnimationVisualEvent(AnimationEvent ev)
        {
            if(AnimatorWrapper != null)
            {
                AnimatorWrapper.OnAnimationVisualEvent(ev);
            }
        }

        void OnAnimationAudioEvent(AnimationEvent ev)
        {
            if(AnimatorWrapper != null)
            {
                AnimatorWrapper.OnAnimationAudioEvent(ev);
            }
        }

        void OnEnable()
        {
            if(AnimatorWrapper != null)
            {
                AnimatorWrapper.OnAnimatorEnabled();
            }
        }
    }

    public class UnityAnimatorWrapper : IAnimator, IDisposable
    {
        UnityAnimatorWrapperEventReceiver _receiver;

        public UnityEngine.Animator Animator { get; private set; }

        public event Action<IAnimationEvent> EventTriggered;
        public event Action<IAnimationEvent> VisualEventTriggered;
        public event Action<IAnimationEvent> AudioEventTriggered;
        public event Action AnimatorEnabled;

        public float CurrentStateDuration
        {
            get
            {
                return Animator.GetCurrentAnimatorStateInfo(0).length;
            }
        }

        public UnityAnimatorWrapper(UnityEngine.Animator unityAnimator)
        {
            Animator = unityAnimator;
            if(Animator == null)
            {
                int i = 0;
                i++;
            }
            _receiver = Animator.gameObject.GetComponent<UnityAnimatorWrapperEventReceiver>();
            if(_receiver == null)
            {
                _receiver = Animator.gameObject.AddComponent<UnityAnimatorWrapperEventReceiver>();
            }
            _receiver.AnimatorWrapper = this;
        }

        bool IAnimator.IsInitialized
        {
            get
            {
                return Animator.isInitialized;
            }
        }

        public void OnAnimationEvent(AnimationEvent ev)
        {
            if(EventTriggered != null)
            {
                EventTriggered(ev.ToStandalone());
            }
        }

        public void OnAnimationVisualEvent(AnimationEvent ev)
        {
            if(VisualEventTriggered != null)
            {
                VisualEventTriggered(ev.ToStandalone());
            }
        }

        public void OnAnimationAudioEvent(AnimationEvent ev)
        {
            if(AudioEventTriggered != null)
            {
                AudioEventTriggered(ev.ToStandalone());
            }
        }

        public void OnAnimatorEnabled()
        {
            if(AnimatorEnabled != null)
            {
                AnimatorEnabled();
            }
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(_receiver);
        }

        public void SetInteger(string name, int value)
        {
            Animator.SetInteger(name, value);
        }

        public void SetFloat(string name, float value)
        {
            Animator.SetFloat(name, value);
        }

        public void SetBool(string name, bool value)
        {
            Animator.SetBool(name, value);
        }

        public void SetTrigger(string name)
        {
            Animator.SetTrigger(name);
        }

        public void ResetTrigger(string name)
        {
            Animator.ResetTrigger(name);
        }

        public void Play(string name)
        {
            Animator.Play(name);
        }

        public bool IsName(string name)
        {
            return Animator.GetCurrentAnimatorStateInfo(0).IsName(name);
        }

        public void Update(float dt)
        {
            // Unity animator will update itself
        }
    }
}
