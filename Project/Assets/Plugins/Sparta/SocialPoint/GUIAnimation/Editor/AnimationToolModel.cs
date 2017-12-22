using System.Collections.Generic;
using SocialPoint.GUIControl;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // This class stores the current screen and current animation selected in the tool
    // All the panels can look into the model to interact with the current animation
    public sealed class AnimationToolModel
    {
        public const string AnimationsRootName = "Animations";

        UIViewController _currentScreen;

        public UIViewController CurrentScreen { get { return _currentScreen; } }

        Animation _currentAnimation;

        public Animation CurrentAnimation { get { return _currentAnimation; } }

        public void ResetState()
        {
            ResetScreen();
        }

        void ResetScreen()
        {
            _currentScreen = null;
            _currentAnimation = null;
        }

        public List<UIViewController> FindScreens()
        {
            return new List<UIViewController>(Object.FindObjectsOfType<UIViewController>());
        }

        public UIViewController GetScreenByIdx(int idx)
        {
            List<UIViewController> screens = FindScreens();

            return idx < screens.Count ? screens[idx] : null;
        }

        public List<Animation> FindAnimations()
        {
            var animations = new List<Animation>();
            Transform animationsRoot = GetAnimationsRoot();
            if(animationsRoot == null)
            {
                return animations;
            }

            for(int i = 0; i < animationsRoot.childCount; ++i)
            {
                Transform child = animationsRoot.GetChild(i);
                Animation animation = child.GetComponent<Animation>();
                if(animation != null)
                {
                    animations.Add(animation);
                }
            }

            return animations;
        }

        Transform GetCreateAnimationRoot()
        {
            if(_currentScreen == null)
            {
                return null;
            }

            Transform animationsRoot = GetAnimationsRoot();
            if(animationsRoot == null)
            {
                GameObject go = AnchorUtility.CreateParentTransform(AnimationsRootName).gameObject;
                animationsRoot = go.transform;
                animationsRoot.SetParent(_currentScreen.transform, false);
            }

            return animationsRoot;
        }

        Transform GetAnimationsRoot()
        {
            return _currentScreen == null ? null : _currentScreen.transform.Find(AnimationsRootName);
        }

        public Animation GetAnimationByIdx(int idx)
        {
            List<Animation> animations = FindAnimations();

            if(animations.Count > 0 && idx < animations.Count)
            {
                return animations[idx];
            }
            return null;
        }

        public void SetCurrentScreen(UIViewController screen)
        {
            ResetScreen();

            _currentScreen = screen;

            // Prepare Screen Transform
#if !NGUI
            AnchorUtility.ConvertTransformToRectTransform(screen.transform);
            AnchorUtility.SetStretchedAnchors(screen.transform);
#endif

            // Set first animation by default
            List<Animation> animations = FindAnimations();
            if(animations.Count > 0)
            {
                _currentAnimation = animations[0];
            }
        }

        public void SetCurrentAnimation(Animation animation)
        {
            _currentAnimation = animation;
        }

        public void RemoveCurrentAnimation()
        {
            if(_currentAnimation != null)
            {
                Object.DestroyImmediate(_currentAnimation.gameObject);
            }

            _currentAnimation = null;
        }

        public Animation CreateAnimation<T>(string animationName) where T : Step
        {
            GameObject animationGo = AnchorUtility.CreateParentTransform(animationName).gameObject;
            animationGo.transform.SetParent(GetCreateAnimationRoot(), false);

            Animation animation = animationGo.AddComponent<Animation>();
            animation.gameObject.name = animationName;

            T rootAnimItem = animation.SetRootAnimationItem<T>();
            rootAnimItem.StepName = animationName;

            animation.RefreshAndInit();

            rootAnimItem.SetStartTime(0f, AnimTimeMode.Global);
            rootAnimItem.SetEndTime(100f, AnimTimeMode.Global);
            rootAnimItem.SetSlot(0);

            return animation;
        }

        public Animation DuplicateCurrentAnimation(string animationName)
        {
            Animation newAnimation = CreateAnimation<Group>("duplicate");
            newAnimation.Copy(CurrentAnimation);
            newAnimation.AnimationName = animationName;

            return newAnimation;
        }

        public void InvertAnimation()
        {
            CurrentAnimation.Invert();
        }

        public void RefreshScreen()
        {
#if NGUI
			UIPanel panel = CurrentScreen.GetComponentInChildren<UIPanel>();
			if(panel)
			{
				panel.Refresh();
			}
#else
#endif
        }
    }
}
