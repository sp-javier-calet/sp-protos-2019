using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryCanvasWindow : EditorWindow
    {
        private static GrayboxLibraryCanvasWindow _window;
        private static Object[] _canvasList;
        private static GrayboxAsset _asset;
        private Vector2 _scrollPos;


        public static void Launch(Object[] canvasList, GrayboxAsset asset)
        {
            _window = (GrayboxLibraryCanvasWindow)ScriptableObject.CreateInstance<GrayboxLibraryCanvasWindow>();
            _canvasList = canvasList;
            _asset = asset;
            _window.ShowUtility();
            _window.titleContent.text = "Graybox UI";
            _window.minSize = new Vector2(350,200);
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(10));

            GUILayout.Label("There are more than one canvas in the scene.\nPlease, select a canvas in where you want to place\nthe asset '"+ _asset.Name + "'");

            GUILayout.Label("", GUILayout.Height(20));
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(false));

            Object[] invertedCanvasList = new Object[_canvasList.Length];
            for(int i = 0; i < _canvasList.Length; i++)
                invertedCanvasList[invertedCanvasList.Length - i - 1] = _canvasList[i];

            foreach (Object canvas in invertedCanvasList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.ExpandWidth(true));
                if (GUILayout.Button(canvas.name, GUILayout.Width(200)))
                {
                    GrayboxLibraryWindow.DownloadAsset(_asset, ((Canvas)canvas).transform);
                    _window.Close();
                }
                GUILayout.Label("", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.Height(10));
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Label("", GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(10));
            if (GUILayout.Button("Cancel", GUILayout.Width(100)))
                _window.Close();
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.Label("", GUILayout.Height(10));

            GUILayout.EndVertical();
        }

        void Update()
        {
            if (EditorApplication.isCompiling)
            {
                _window.Close();
            }
        }
    }
}