using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SocialPoint.GUIControl;

namespace SocialPoint.GUIAnimation
{
    // Entry EditorWindows for the animation tool
    public class GUIAnimationTool : EditorWindow
    {
        AnimationToolModel _animationModel = new AnimationToolModel();

        public AnimationToolModel AnimationModel { get { return _animationModel; } }

        static AnimationBlackboard _blackBoard = new AnimationBlackboard();

        public static AnimationBlackboard Blackboard { get { return _blackBoard; } }

        static KeyboardController _keyController = new KeyboardController();

        public static KeyboardController KeyController { get { return _keyController; } }

        static MouseController _mouseController = new MouseController();

        public static MouseController MouseController { get { return _mouseController; } }

        static AnimationEditorPlayer _animationEditorPlayer = new AnimationEditorPlayer();

        public static AnimationEditorPlayer AnimationEditorPlayer { get { return _animationEditorPlayer; } }

        AnimationEditorContainer _animationEditorContainer = new AnimationEditorContainer();

        int _currentScreenIdx = 0;
        int _currentAnimationIdx = 0;

        bool _isInit = false;

        [MenuItem("Social Point/GUI/Animation Tool")]
        public static void ShowWindow()
        {
            GUIAnimationTool animationTool = EditorWindow.GetWindow<GUIAnimationTool>();
            animationTool.titleContent = new GUIContent("Animation Tool");
        }

        public void Init()
        {
            ResetState();
        }

        public void SaveState()
        {
            if(AnimationModel.CurrentScreen != null)
            {
                AnimationPrefabUtility.SaveScreenPrefab(AnimationModel.CurrentScreen.gameObject);
            }
        }

        void ResetState()
        {
            _animationModel.ResetState();
            _animationEditorContainer.ResetState();

            _currentAnimationIdx = 0;
            _currentScreenIdx = 0;
            Blackboard.ResetState();
            KeyController.ResetState();
            MouseController.ResetState();

            _animationEditorPlayer.Init(this);
        }

        void Update()
        {
            if(Application.isPlaying)
            {
                return;
            }
        }

        void OnGUI()
        {
            DoUpdate();
            DoRender();
        }

        void DoUpdate()
        {
            if(!_isInit)
            {
                _isInit = true;
                Init();
            }

            KeyController.UpdateState();
            MouseController.UpdateState();
            _animationEditorPlayer.Update(this);

            TryResetPanelOnFocus();
        }

        void TryResetPanelOnFocus()
        {
            if(Event.current.type == EventType.mouseDown)
            {
                Blackboard.Remove(AnimationBlackboardKey.FocusPanelKey);
            }
        }

        void DoRender()
        {
            GUILayout.BeginVertical();
            List<UIViewController> screens = AnimationModel.FindScreens();

            if(screens.Count == 0)
            {
                if(Event.current.type == EventType.Repaint)
                {
                    ResetState();
                    RenderNoScreenMessage();
                }
                return;
            }

            RenderScreensSelector(screens);
            if(AnimationModel.CurrentScreen == null)
            {
                if(Event.current.type == EventType.Repaint)
                {
                    ResetState();
                }
                return;
            }

            List<Animation> animations = _animationModel.FindAnimations();

            if(animations.Count == 0)
            {
                _animationModel.RemoveCurrentAnimation();
            }
			
            GUILayout.BeginHorizontal();
			
            RenderAnimationSelection(animations);
            RenderActionButtons();
			
            GUILayout.EndHorizontal();
			
            GUILayout.EndVertical();
			
            if(_animationModel.CurrentAnimation == null)
            {
                return;
            }
			
            _animationEditorContainer.Render(this);
        }

        void RenderNoScreenMessage()
        {
            if(Event.current.type == EventType.Layout)
            {
                GUILayout.Label("No screens found. Add the screen prefab to the current scene.", EditorStyles.helpBox);
            }
        }

        void RenderScreensSelector(List<UIViewController> screens)
        {
            List<string> animationsRootOptionList = AnimationToolUtility.ComponentsToNames<UIViewController>(screens);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Screen:", GUILayout.MaxWidth(80f));
            _currentScreenIdx = EditorGUILayout.Popup(_currentScreenIdx, animationsRootOptionList.ToArray(), GUILayout.MaxWidth(160f));
            GUILayout.EndHorizontal();

            UIViewController currentScreen = _animationModel.GetScreenByIdx(_currentScreenIdx);
            if(currentScreen != _animationModel.CurrentScreen)
            {
                _animationModel.SetCurrentScreen(currentScreen);

                // Reset the state
                _animationEditorContainer.ResetState();
            }
        }

        void RenderAnimationSelection(List<Animation> animations)
        {
            List<string> animationsOptionList = AnimationToolUtility.ComponentsToNames<Animation>(animations);

            GUILayout.Label("Animation:", GUILayout.MaxWidth(80f), GUILayout.ExpandWidth(false));
            _currentAnimationIdx = EditorGUILayout.Popup(_currentAnimationIdx, animationsOptionList.Count > 0 ? animationsOptionList.ToArray() : new string[1]{ "" }, GUILayout.MaxWidth(160f), GUILayout.ExpandWidth(false));

            Animation currentAnimation = _animationModel.GetAnimationByIdx(_currentAnimationIdx);
            if(currentAnimation != _animationModel.CurrentAnimation)
            {
                _animationModel.SetCurrentAnimation(currentAnimation);
                _animationEditorContainer.ResetState();
            }
        }

        void RenderActionButtons()
        {
            if(GUILayout.Button(_animationEditorPlayer.IsPlaying() ? "Stop" : "Play", GUILayout.ExpandWidth(false), GUILayout.Width(50f)))
            {
                if(_animationEditorPlayer.IsPlaying())
                {
                    _animationEditorPlayer.Stop();
                }
                else
                {
                    _animationEditorPlayer.Play();
                }
            }

            if(GUILayout.Button("Create", GUILayout.ExpandWidth(false)))
            {
                EnterStringPopup createNameWindow = (EnterStringPopup)EditorWindow.GetWindow(typeof(EnterStringPopup));
                createNameWindow.titleContent = new GUIContent("Animation Name");
                createNameWindow.SetCallbacks(() => {
                    OnAnimationNameCreated(createNameWindow.Value);
                });
            }

            UnityEngine.GUI.enabled = AnimationModel.CurrentScreen != null;
            if(GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
            {
                SaveState();
            }
            UnityEngine.GUI.enabled = true;

            UnityEngine.GUI.enabled = AnimationModel.CurrentAnimation != null;
            if(GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
            {
                ConfirmationPopup confirmationPopup = (ConfirmationPopup)EditorWindow.GetWindow(typeof(ConfirmationPopup));
                confirmationPopup.titleContent = new GUIContent("Remove Animation");
                confirmationPopup.SetTitle(string.Format("Really want to Remove {0} ?", AnimationModel.CurrentAnimation.name));
                confirmationPopup.SetCallbacks(() => {
                    AnimationModel.RemoveCurrentAnimation();
                    ResetState();
                });
            }
            UnityEngine.GUI.enabled = true;

            UnityEngine.GUI.enabled = AnimationModel.CurrentAnimation != null;
            if(GUILayout.Button("Rename", GUILayout.ExpandWidth(false)))
            {
                EnterStringPopup renamePopup = (EnterStringPopup)EditorWindow.GetWindow(typeof(EnterStringPopup));
                renamePopup.titleContent = new GUIContent("Rename Animation");
                renamePopup.SetTitle(string.Format("Set the new animation name for {0} ?", AnimationModel.CurrentAnimation.AnimationName));
                renamePopup.SetCallbacks(() => {
                    AnimationModel.CurrentAnimation.AnimationName = renamePopup.Value;
                });
            }
            UnityEngine.GUI.enabled = true;

            UnityEngine.GUI.enabled = AnimationModel.CurrentAnimation != null;
            if(GUILayout.Button("Duplicate", GUILayout.ExpandWidth(false)))
            {
                EnterStringPopup renamePopup = (EnterStringPopup)EditorWindow.GetWindow(typeof(EnterStringPopup));
                renamePopup.titleContent = new GUIContent("Duplicate Animation");
                renamePopup.SetTitle(string.Format("Set the new animation name to duplicate {0} ?", AnimationModel.CurrentAnimation.AnimationName));
                renamePopup.SetCallbacks(() => {
                    Animation animation = AnimationModel.DuplicateCurrentAnimation(renamePopup.Value);

                    List<Animation> animations = _animationModel.FindAnimations();
                    _currentAnimationIdx = animations.Count - 1;
                    _animationModel.SetCurrentAnimation(animation);
					
                    _animationEditorContainer.ResetState();
                });
            }
            UnityEngine.GUI.enabled = true;

            UnityEngine.GUI.enabled = AnimationModel.CurrentAnimation != null;
            if(GUILayout.Button("Invert", GUILayout.ExpandWidth(false)))
            {
                AnimationModel.CurrentAnimation.Invert();
            }
            UnityEngine.GUI.enabled = true;
        }

        void OnAnimationNameCreated(string animationName)
        {
            Animation animation = _animationModel.CreateAnimation<Group>(animationName);

            List<Animation> animations = _animationModel.FindAnimations();
            _currentAnimationIdx = animations.Count - 1;
            _animationModel.SetCurrentAnimation(animation);

            _animationEditorContainer.ResetState();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

    }
}
