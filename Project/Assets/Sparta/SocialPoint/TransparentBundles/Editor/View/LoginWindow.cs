using UnityEditor;
using UnityEngine;
using System.Collections;
using System;


namespace SocialPointEditor.TransparentBundles
{
    public class LoginWindow : EditorWindow
    {
        public const string LOGIN_PREF_KEY = "TBLoginUser";
        private Action callback, cancelCallback;
        private string loginUser;
        private string error;
        private bool closedProperly = false;


        [MenuItem("SocialPoint/Change Login")]
        public static void ChangeLogin()
        {
            //We just login
            Open(TransparentBundleAPI.Login);
        }

        public static void Open(Action callback, Action cancelCallback = null, string errorMessage = "")
        {
            var window = GetWindow<LoginWindow>(true, "Transparent Bundles Login", true);
            window.callback = callback;
            window.cancelCallback = cancelCallback;
            var rect = new Rect(750, 400, 400, 100);
            window.position = rect;
            var size = new Vector2(rect.width, rect.height);
            window.maxSize = size;
            window.minSize = size;
            window.loginUser = EditorPrefs.GetString(LOGIN_PREF_KEY);
            window.error = errorMessage;
        }

        void OnGUI()
        {
            if(string.IsNullOrEmpty(error))
            {
                EditorGUILayout.HelpBox("This is the username for transparent bundles, use your socialpoint email, if you can't access please contact the Tools department", MessageType.Info);
            } else
            {
                EditorGUILayout.HelpBox("Error found: " + error, MessageType.Error);
            }
            loginUser = EditorGUILayout.TextField("Login", loginUser);

            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Save And Login"))
            {
                EditorPrefs.SetString(LOGIN_PREF_KEY, loginUser);
                closedProperly = true;

                if(callback != null)
                {
                    callback();
                }
                Close();
            }
        }

        public void OnDestroy()
        {
            if(!closedProperly)
            {
                Debug.LogError("Login cancelled, the process won't continue");
                cancelCallback();
            }
        }

    }
}
