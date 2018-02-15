using System.Collections.Generic;
using SocialPoint.GUIControl;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // Entry EditorWindows for the animation tool
    public sealed class GUIAnimationTool : EditorWindow
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

        static AnimationEditorContainer _animationEditorContainer = new AnimationEditorContainer();

        bool _repaint;
        double _nextRepaintTime;
        const double _repaintMinTime = 2.0;

        bool _saveState;
        double _nextSaveTime;
        const double _saveMinTime = 10.0;

        int _currentScreenIdx;
        int _currentAnimationIdx;

        bool _isInit;

        [MenuItem("Sparta/GUI/Animation Tool", false, 500)]
        public static void ShowWindow()
        {
            GUIAnimationTool animationTool = EditorWindow.GetWindow<GUIAnimationTool>();
            animationTool.titleContent = new GUIContent("Animation Tool");
        }

        public void Init()
        {
            ResetState();
        }

        public static void ResetTimeLine()
        {
            AnimationEditorContainer.ResetTimeLine();
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

        void OnGUI()
        {
            KeyController.UpdateState();
            MouseController.UpdateState();
            
            bool doRepaintGUI = Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDrag;
            if(doRepaintGUI)
            {
                ForceRepaint();
            }

            if(Event.current.type == EventType.Repaint)
            {
                List<UIViewController> screens = AnimationModel.FindScreens();
                if(screens.Count == 0)
                {
                    ResetState();
                    RenderNoScreenMessage();
                    ForceRepaint();

                    return;
                }
                DoUpdate();
            }

            DoRender();
        }

        void DoUpdate()
        {
            if(!_isInit)
            {
                _isInit = true;
                Init();
            }
            _animationEditorPlayer.Update(this);

            TryResetPanelOnFocus();
        }

        static void TryResetPanelOnFocus()
        {
            if(Event.current.type == EventType.MouseDown)
            {
                Blackboard.Remove(AnimationBlackboardKey.FocusPanelKey);
            }
        }

        void DoRender()
        {
            GUILayout.BeginVertical();
            List<UIViewController> screens = AnimationModel.FindScreens();

            RenderScreensSelector(screens);
            if(Event.current.type == EventType.Repaint && AnimationModel.CurrentScreen == null)
            {
                ResetState();
                ForceRepaint();

                return;
            }

            List<Animation> animations = _animationModel.FindAnimations();
            if(Event.current.type == EventType.Repaint && animations.Count == 0)
            {
                _animationModel.RemoveCurrentAnimation();
                ForceRepaint();
            }

            GUILayout.BeginHorizontal();

            RenderAnimationSelection(animations);
            RenderActionButtons();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if(_animationModel.CurrentAnimation == null)
            {
                ForceRepaint();
                return;
            }

            _animationEditorContainer.Render(this);
        }

        static void RenderNoScreenMessage()
        {
            GUILayout.Label("No screens found. Add the screen prefab to the current scene.", EditorStyles.helpBox);
        }

        void RenderScreensSelector(List<UIViewController> screens)
        {
            List<string> animationsRootOptionList = AnimationToolUtility.ComponentsToNames<UIViewController>(screens);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Screen:", GUILayout.MaxWidth(80f));
            _currentScreenIdx = EditorGUILayout.Popup(_currentScreenIdx, animationsRootOptionList.ToArray(), GUILayout.MaxWidth(160f));
            GUILayout.EndHorizontal();

            UIViewController currentScreen = _animationModel.GetScreenByIdx(_currentScreenIdx);
            if(Event.current.type == EventType.Repaint && currentScreen != _animationModel.CurrentScreen)
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
            _currentAnimationIdx = EditorGUILayout.Popup(_currentAnimationIdx, animationsOptionList.Count > 0 ? animationsOptionList.ToArray() : new string[]{ "" }, GUILayout.MaxWidth(160f), GUILayout.ExpandWidth(false));

            Animation currentAnimation = _animationModel.GetAnimationByIdx(_currentAnimationIdx);
            if(Event.current.type == EventType.Repaint && currentAnimation != _animationModel.CurrentAnimation)
            {
                ResetTimeLine();

                _animationModel.SetCurrentAnimation(currentAnimation);
                _animationEditorContainer.ResetState();

                CleanGarbageCollector();
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
                var createNameWindow = (EnterStringPopup)EditorWindow.GetWindow(typeof(EnterStringPopup));
                createNameWindow.titleContent = new GUIContent("Animation Name");
                createNameWindow.SetCallbacks(() => OnAnimationNameCreated(createNameWindow.Value));
            }

            GUI.enabled = AnimationModel.CurrentScreen != null;
            if(GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
            {
                SaveState(true);
            }
            GUI.enabled = true;

            GUI.enabled = AnimationModel.CurrentAnimation != null;
            if(GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
            {
                var confirmationPopup = (ConfirmationPopup)EditorWindow.GetWindow(typeof(ConfirmationPopup));
                confirmationPopup.titleContent = new GUIContent("Remove Animation");
                confirmationPopup.SetTitle(string.Format("Really want to Remove {0} ?", AnimationModel.CurrentAnimation.name));
                confirmationPopup.SetCallbacks(() => {
                    AnimationModel.RemoveCurrentAnimation();
                    ResetState();
                });
            }
            GUI.enabled = true;

            GUI.enabled = AnimationModel.CurrentAnimation != null;
            if(GUILayout.Button("Rename", GUILayout.ExpandWidth(false)))
            {
                var renamePopup = (EnterStringPopup)EditorWindow.GetWindow(typeof(EnterStringPopup));
                renamePopup.titleContent = new GUIContent("Rename Animation");
                renamePopup.SetTitle(string.Format("Set the new animation name for {0} ?", AnimationModel.CurrentAnimation.AnimationName));
                renamePopup.SetCallbacks(() => {
                    AnimationModel.CurrentAnimation.AnimationName = renamePopup.Value;
                });
            }
            GUI.enabled = true;

            GUI.enabled = AnimationModel.CurrentAnimation != null;
            if(GUILayout.Button("Duplicate", GUILayout.ExpandWidth(false)))
            {
                var renamePopup = (EnterStringPopup)EditorWindow.GetWindow(typeof(EnterStringPopup));
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
            GUI.enabled = true;

            GUI.enabled = AnimationModel.CurrentAnimation != null;
            if(GUILayout.Button("Invert", GUILayout.ExpandWidth(false)))
            {
                AnimationModel.CurrentAnimation.Invert();
            }
            GUI.enabled = true;
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
            if(AnimationModel.CurrentAnimation != null)
            {
                AnimationModel.CurrentAnimation.Init();
            }

            if(_repaint || EditorApplication.timeSinceStartup > _nextRepaintTime)
            {
                Repaint();

                _nextRepaintTime = (float)EditorApplication.timeSinceStartup + _repaintMinTime;
                _repaint = false;
            }

            if(_saveState && (EditorApplication.timeSinceStartup > _nextSaveTime))
            {
                DoSaveState();

                _saveState = false;
            }
        }

        public void ForceRepaint()
        {
            _repaint = true;
        }

        public void SaveState(bool isImmediate = false)
        {
            if(isImmediate)
            {
                DoSaveState();
            }
            else
            {
                _saveState = true;
                _nextSaveTime = EditorApplication.timeSinceStartup + _saveMinTime;
            }
        }

        void DoSaveState()
        {
            if(AnimationModel.CurrentScreen != null)
            {
                AnimationPrefabUtility.SaveScreenPrefab(AnimationModel.CurrentScreen.gameObject);
            }
        }

        static void CleanGarbageCollector()
        {
            System.GC.Collect();
        }
    }
}
