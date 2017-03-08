using UnityEngine;
using UnityEditor;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibrarySmbSetup : EditorWindow
    {
        public static GrayboxLibrarySmbSetup Window;
        private string _psw = "";

        public static void Launch()
        {
            Window = (GrayboxLibrarySmbSetup)EditorWindow.GetWindow<GrayboxLibrarySmbSetup>();
            Window.ShowPopup();
            Window.Focus();
            Window.position = new Rect(300, 200, 300, 120);
            Window.maxSize = new Vector2(300, 120);
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Graybox Library");
            GUILayout.Label("", GUILayout.Height(10));

            GUILayout.Label("Please, specify the Mac password");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Mac password:", GUILayout.ExpandWidth(false));
            _psw = EditorGUILayout.PasswordField(_psw, GUILayout.ExpandWidth(true));
            GUILayout.Label("", GUILayout.Width(50), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(10));

            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            if(GUILayout.Button("Save", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
            {
                Window.Close();
                PlayerPrefs.SetString(GrayboxLibraryConfig.SuPswPlayerPerfs, _psw);
                PlayerPrefs.Save();
                GrayboxLibraryWindow.LaunchClient();
            }
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}