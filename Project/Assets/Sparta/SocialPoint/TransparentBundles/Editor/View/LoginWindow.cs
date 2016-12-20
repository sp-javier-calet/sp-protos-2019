using UnityEditor;
using UnityEngine;
using System.Collections;
using System;


namespace SocialPointEditor.TransparentBundles
{
    public class LoginWindow : EditorWindow
    {
        public const string LOGIN_PREF_KEY = "TBLoginUser";
        private Action _callback, _cancelCallback;
        private string _loginUser;
        private string _error;
        private bool _closedProperly = false;


        [MenuItem("SocialPoint/Change Login")]
        public static void ChangeLogin()
        {
            //We just login
            Open(TransparentBundleAPI.Login);
        }

        public static void Open(Action callback, Action cancelCallback = null, string errorMessage = "")
        {
            var window = GetWindow<LoginWindow>(true, "Transparent Bundles Login", true);
            window._callback = callback;
            window._cancelCallback = cancelCallback;
            var rect = new Rect(750, 400, 400, 100);
            window.position = rect;
            var size = new Vector2(rect.width, rect.height);
            window.maxSize = size;
            window.minSize = size;
            window._loginUser = EditorPrefs.GetString(LOGIN_PREF_KEY);
            window._error = errorMessage;
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

            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Save And Login"))
            {
                EditorPrefs.SetString(LOGIN_PREF_KEY, _loginUser);
                _closedProperly = true;

                if(_callback != null)
                {
                    _callback();
                }
                Close();
            }
        }

        public void OnDestroy()
        {
            if(!_closedProperly)
            {
                Debug.LogError("Login cancelled, the process won't continue");
                _cancelCallback();
            }
        }

    }
}
