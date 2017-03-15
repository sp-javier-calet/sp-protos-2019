using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SocialPoint.TransparentBundles
{
    public class ServerInfoWindow : EditorWindow
    {
        public static ServerInfoWindow Window;
        static EditorClientController _controller;
        static Vector2 _scrollPos;

        static void Init()
        {
            _controller = EditorClientController.GetInstance();
            _scrollPos = Vector2.zero;
        }
        
        public static void OpenWindow()
        {
            Window = (ServerInfoWindow)EditorWindow.GetWindow(typeof(ServerInfoWindow));
            Window.titleContent.text = "Bundles";
            Init();
        }

        void OnGUI()
        {
            if(Window == null)
            {
                OpenWindow();
            }
            else if(_controller == null)
            {
                Init();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(10));

            EditorGUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(10));

            EditorGUILayout.BeginHorizontal();
            Rect serverIconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
            GUI.DrawTexture(serverIconRect, _controller.DownloadImage(Config.IconsPath + Config.ServerDbImageName));
            GUILayout.Label("Operations in Server");
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(10));

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, BundlesWindow.BodyStyle, GUILayout.ExpandHeight(true));
            GUILayout.Label("", GUILayout.Height(5));

            var queueEnum = _controller.ServerInfo.ProcessingQueue.GetEnumerator();
            for(int i = 0; queueEnum.MoveNext(); i++)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Height(25), GUILayout.ExpandHeight(false));
                GUILayout.Label("", GUILayout.Width(5));
                if(i > 0)
                {
                    if(GUILayout.Button("Cancel", GUILayout.Width(50)))
                    {
                        _controller.CancelBundleOperation(queueEnum.Current.Key);
                    }
                }
                BundlesWindow.DrawOperationIcon(queueEnum.Current.Value, i == 0);
                string bundleNames = "";
                int j = 0;
                while(j < BundlesWindow.BundleList.Count && bundleNames.Length < Window.position.width)
                {
                    if(BundlesWindow.BundleList[j].OperationQueue.ContainsKey(queueEnum.Current.Key))
                    {
                        bundleNames += BundlesWindow.BundleList[j].Asset.Name + ", ";
                    }
                    j++;
                }
                if(bundleNames.Length == 0)
                {
                    //TEMPORARY
                    var newBundlesEnum = _controller.NewBundles.GetEnumerator();
                    while(newBundlesEnum.MoveNext())
                    {
                        bundleNames += newBundlesEnum.Current.Key + ", ";
                    }
                    newBundlesEnum.Dispose();
                }
                if(bundleNames.Length > 0)
                {
                    bundleNames = bundleNames.Substring(0, bundleNames.Length - 2);
                }
                if(j < BundlesWindow.BundleList.Count)
                {
                    bundleNames += " (" + (BundlesWindow.BundleList.Count - j).ToString() + " more)";
                }
                GUILayout.Label(bundleNames, BundlesWindow.BodyTextStyle, GUILayout.ExpandWidth(true), GUILayout.Height(25));
                GUILayout.Label("", GUILayout.Width(5));
                EditorGUILayout.EndHorizontal();
                GUILayout.Label("", GUILayout.Height(5));
            }
            queueEnum.Dispose();

            EditorGUILayout.EndScrollView();

            GUILayout.Label("", GUILayout.Height(15));

            EditorGUILayout.EndVertical();

            GUILayout.Label("", GUILayout.Width(10));

            EditorGUILayout.EndHorizontal();
        }

        void Update()
        {
            Repaint();
        }
    }
}
