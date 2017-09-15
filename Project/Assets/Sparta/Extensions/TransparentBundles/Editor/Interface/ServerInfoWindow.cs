using UnityEditor;
using UnityEngine;

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

            EditorGUILayout.BeginVertical(BundlesWindow.BodyStyle, GUILayout.ExpandHeight(true));
            GUILayout.Label("", GUILayout.Height(10));

            if(_controller.ServerInfo.ProgressMessage.Length > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(5));
                Rect progressRectBar = GUILayoutUtility.GetRect(0, 25, GUILayout.ExpandWidth(true));
                Rect progressRect = new Rect(progressRectBar.position.x, progressRectBar.position.y, progressRectBar.width * _controller.ServerInfo.Progress, progressRectBar.height);
                Rect progressMessageRect = new Rect(progressRectBar.position.x + 5, progressRectBar.position.y, progressRectBar.width - 10, progressRectBar.height);
                GUI.DrawTexture(progressRectBar, _controller.DownloadImage(Config.IconsPath + Config.ProgressBarBkgImageName));
                GUI.DrawTexture(progressRect, _controller.DownloadImage(Config.IconsPath + Config.ProgressBarImageName));
                GUI.Label(progressMessageRect, Mathf.RoundToInt(_controller.ServerInfo.Progress * 100).ToString() + "%    " + _controller.ServerInfo.ProgressMessage, BundlesWindow.BodyTextStyle);
                GUILayout.Label("", GUILayout.Width(5));
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Label("", GUILayout.Height(10));
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            using(var queueEnum = _controller.ServerInfo.ProcessingQueue.GetEnumerator())
            {
                for(int i = 0; queueEnum.MoveNext(); i++)
                {
                    GUILayout.Label("", GUILayout.Height(5));
                    EditorGUILayout.BeginHorizontal(GUILayout.Height(25), GUILayout.ExpandHeight(false));
                    GUILayout.Label("", GUILayout.Width(5));
                    if(i > 0)
                    {
                        if(queueEnum.Current.Value.AuthorMail == EditorPrefs.GetString(LoginWindow.LOGIN_PREF_KEY) && GUILayout.Button("Cancel", GUILayout.Width(50)))
                        {
                            _controller.CancelBundleOperation(queueEnum.Current.Key);
                        }
                    }
                    BundlesWindow.DrawOperationIcon(queueEnum.Current.Value.Operation, i == 0);
                    string bundleNames = "";
                    int j = 0;
                    while(j < BundlesWindow.BundleList.Count && bundleNames.Length < Window.position.width)
                    {
                        if(BundlesWindow.BundleList[j].OperationQueue.ContainsKey(queueEnum.Current.Key))
                        {
                            bundleNames += BundlesWindow.BundleList[j].Name.Substring(0, BundlesWindow.BundleList[j].Name.LastIndexOf("_")) + ", ";
                        }
                        j++;
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
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

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
