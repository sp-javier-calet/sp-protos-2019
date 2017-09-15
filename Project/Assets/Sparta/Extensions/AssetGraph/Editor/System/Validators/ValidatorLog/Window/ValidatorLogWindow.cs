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
        Dictionary<Type, WindowView<ValidatorLogWindow>> _availableViews = new Dictionary<Type, WindowView<ValidatorLogWindow>>();

        WindowView<ValidatorLogWindow> _currentView;

        [MenuItem(AssetBundleGraphSettings.GUI_TEXT_MENU_OPEN_VALIDATION, false, 2)]
        public static void Open()
        {
            GetWindow<ValidatorLogWindow>("Validator Log");
        }

        public T GetView<T>() where T : WindowView<ValidatorLogWindow>
        {
            return (T)_availableViews[typeof(T)];
        }

        public T ChangeView<T>() where T : WindowView<ValidatorLogWindow>
        {
            _currentView = _availableViews[typeof(T)];
            _currentView.OnEnableMethod();

            return (T)_currentView;
        }

        void OnEnable()
        {
            minSize = new Vector2(800, 100);
            _availableViews.Add(typeof(ValidatorView), new ValidatorView(this));
            _availableViews.Add(typeof(ValidatorSelectView), new ValidatorSelectView(this));

            _currentView = GetView<ValidatorView>();
        }

        void OnFocus()
        {
            _currentView.OnFocusMethod();
        }

        private void OnGUI()
        {
            DrawValidatorSelectionBar();
            _currentView.OnGUIMethod();
        }

        public void DrawValidatorSelectionBar()
        {
            using(var h = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var style = EditorStyles.toolbarButton;
                style.richText = true;

                var currentLog = GetView<ValidatorView>().CurrentLogInWindow;

                var source = currentLog.IsLocal ? "<color=yellow>Local</color> " : "<color=yellow>Remote</color> ";

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

                var selectView = GUI.Toggle(rectRight, _currentView is ValidatorSelectView, "Select Validation Report", style);

                if(selectView != _currentView is ValidatorSelectView)
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
