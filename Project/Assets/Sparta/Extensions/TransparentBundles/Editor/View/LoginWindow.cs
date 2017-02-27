using System;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.TransparentBundles
{
    public class LoginWindow : EditorWindow
    {
        public const string LOGIN_PREF_KEY = "TBLoginUser";
        Action _callback;
        Action _cancelCallback;
        string _loginUser;
        string _error;
        bool _closedProperly;
        TBConfig _config;
        string _project;

        /// <summary>
        /// Opens the login window and sets the callbacks
        /// </summary>
        /// <param name="callback">Callback executed when the login button is pressed.</param>
        /// <param name="cancelCallback">Callback executed when exiting the window by closing it without logging</param>
        /// <param name="errorMessage">Optional error message to display at the top of the window</param>
        public static void Open(Action callback, Action cancelCallback = null, string errorMessage = "")
        {
            var window = GetWindow<LoginWindow>(true, "Transparent Bundles Login", true);
            window._callback = callback;
            window._cancelCallback = cancelCallback;
            var rect = new Rect(750, 400, 400, 120);
            window.position = rect;
            var size = new Vector2(rect.width, rect.height);
            window.maxSize = size;
            window.minSize = size;
            window._loginUser = EditorPrefs.GetString(LOGIN_PREF_KEY);
            window._error = errorMessage;
            window._config = TBConfig.GetConfig();
            window._project = window._config.project;
        }

        void OnGUI()
        {
            if(string.IsNullOrEmpty(_error))
            {
                EditorGUILayout.HelpBox("This is the username for transparent bundles, use your socialpoint email, if you can't access please contact the Tools department", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Error found: " + _error, MessageType.Error);
            }
            _loginUser = EditorGUILayout.TextField("Login", _loginUser);
            _project = EditorGUILayout.TextField("Project", _project);

            GUILayout.FlexibleSpace();

            GUI.enabled = !string.IsNullOrEmpty(_loginUser) && !string.IsNullOrEmpty(_project);

            if(GUILayout.Button("Save And Login"))
            {
                if(_config.project != _project)
                {
                    _config.project = _project;
                    EditorUtility.SetDirty(_config);
                    AssetDatabase.Refresh();
                }

                EditorPrefs.SetString(LOGIN_PREF_KEY, _loginUser);
                _closedProperly = true;

                if(_callback != null)
                {
                    _callback();
                }
                Close();
            }
            GUI.enabled = true;
        }

        public void OnDestroy()
        {
            if(!_closedProperly)
            {
                //if this has been destroyed without logging first.
                if(_cancelCallback != null)
                {
                    _cancelCallback();
                }
            }
        }

    }
}
