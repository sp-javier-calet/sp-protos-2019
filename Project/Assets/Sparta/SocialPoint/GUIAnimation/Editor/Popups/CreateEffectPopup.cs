using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public sealed class CreateEffectPopup : EditorWindowCallback
    {
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(EnterStringPopup));
        }

        public void SetTitle(string title)
        {
            _title = title;
        }

        public System.Type Value;

        string _title = "Select The Action";

        List<StepData> _actions = new List<StepData>();

        Vector2 _scrollPos = Vector2.zero;

        public void SetAnimItemType(StepType type)
        {
            switch(type)
            {
            case StepType.BlendEffect:
                FillBlendingActions();
                break;
            case StepType.TriggerEffect:
                FillInstantActions();
                break;
            }
        }

        void FillBlendingActions()
        {
            _actions.Clear();
            _actions = StepsManager.BlendStepsData;
        }

        void FillInstantActions()
        {
            _actions.Clear();
            _actions = StepsManager.TriggerStepsData;	
        }

        void OnGUI()
        {
            GUILayout.Label(_title, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Title, GUI.skin.label, TextAnchor.MiddleCenter));
            GUILayout.Space(10f);
            const float scrollHeight = 150f;

            // List of Triggers
            _scrollPos = GUILayout.BeginScrollView(
                _scrollPos
				, GUILayout.Width(position.width)
				, GUILayout.Height(scrollHeight)
            );

            // Show list of actions
            for(int i = 0; i < _actions.Count; ++i)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Space(position.width * 0.5f - 50f);

                if(GUILayout.Button(StepsManager.GetStepName(_actions[i].StepType), AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, GUI.skin.button), GUILayout.ExpandWidth(false)))
                {
                    Value = _actions[i].StepType;
                    OnAccept();
                    Close();
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            // Cancel Button
            GUILayout.BeginArea(new Rect(0f, scrollHeight + 50f, 200, 50));
            GUILayout.BeginHorizontal();

            GUILayout.Space(position.width * 0.5f - 50f);
            if(GUILayout.Button("Cancel", GUILayout.MaxWidth(100f)))
            {
                OnCancel();
                Close();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
