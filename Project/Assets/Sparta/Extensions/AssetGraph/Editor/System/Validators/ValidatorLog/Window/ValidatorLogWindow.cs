using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace AssetBundleGraph
{
    public abstract class WindowView<T> where T : EditorWindow
    {
        public T parentWindow;
        public abstract void OnEnableMethod();
        public abstract void OnFocusMethod();
        public abstract void OnGUIMethod();

        public WindowView(T parent)
        {
            parentWindow = parent;
        }

    }

    public class ValidatorLogWindow : EditorWindow
    {
        Dictionary<Type, WindowView<ValidatorLogWindow>> availableViews = new Dictionary<Type, WindowView<ValidatorLogWindow>>();

        WindowView<ValidatorLogWindow> currentView;

        [MenuItem("Window/AssetGraph/Open Validation Log", false, 2)]
        public static void Open()
        {
            GetWindow<ValidatorLogWindow>("Validator Log");
        }

        public T GetView<T>() where T : WindowView<ValidatorLogWindow>
        {
            return (T)availableViews[typeof(T)];
        }

        public T ChangeView<T>() where T : WindowView<ValidatorLogWindow>
        {
            currentView = availableViews[typeof(T)];
            currentView.OnEnableMethod();

            return (T)currentView;
        }

        void OnEnable()
        {
            minSize = new Vector2(800, 100);
            availableViews.Add(typeof(ValidatorView), new ValidatorView(this));
            availableViews.Add(typeof(ValidatorSelectView), new ValidatorSelectView(this));

            currentView = GetView<ValidatorView>();
        }

        void OnFocus()
        {
            currentView.OnFocusMethod();
        }

        private void OnGUI()
        {
            DrawValidatorSelectionBar();
            currentView.OnGUIMethod();
        }

        public void DrawValidatorSelectionBar()
        {
            using(var h = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var style = EditorStyles.toolbarButton;
                style.richText = true;

                var currentLog = GetView<ValidatorView>().currentLogInWindow;

                var source = currentLog.isLocal ? "<color=yellow>Local</color> " : "<color=yellow>Remote</color> ";


                var hRect = GUILayoutUtility.GetRect(new GUIContent(), style);

                GUI.Label(hRect, "Current Validation: " + source + currentLog.FormatedDate, style);

                var rectRight = new Rect(hRect);
                rectRight.x = rectRight.width - 150;
                rectRight.width = 150;

                var rectLeft = new Rect(hRect);
                rectLeft.width = 175;
                if(GUI.Button(rectLeft, "Select Last Validation Report", style))
                {

                    ChangeView<ValidatorView>().LoadValidatorLog(ValidatorController.GetLastValidatorLog());
                }

                var selectView = GUI.Toggle(rectRight, currentView is ValidatorSelectView, "Select Validation Report", style);

                if(selectView != currentView is ValidatorSelectView)
                {
                    if(selectView)
                    {
                        ChangeView<ValidatorSelectView>();
                    }
                    else
                    {
                        ChangeView<ValidatorView>();
                    }
                }
            }
        }
    }
}
