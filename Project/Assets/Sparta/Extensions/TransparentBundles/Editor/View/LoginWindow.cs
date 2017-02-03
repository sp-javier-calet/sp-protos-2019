using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace SocialPoint.TransparentBundles
{
    public class LoginWindow : EditorWindow
    {
        public const string LOGIN_PREF_KEY = "TBLoginUser";
        private Action _callback;
        private Action _cancelCallback;
        private string _loginUser;
        private string _error;
        private bool _closedProperly = false;
        private TBConfig _config;

        [MenuItem("SocialPoint/Change Login")]
        public static void ChangeLogin()
        {
            //We just login without callbacks
            Open(() => TransparentBundleAPI.Login());
        }

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
            _config.project = EditorGUILayout.TextField("Project", _config.project);

            GUILayout.FlexibleSpace();

            GUI.enabled = !string.IsNullOrEmpty(_loginUser) && !string.IsNullOrEmpty(_config.project);

            if(GUILayout.Button("Save And Login"))
            {
                AssetDatabase.SaveAssets();
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
