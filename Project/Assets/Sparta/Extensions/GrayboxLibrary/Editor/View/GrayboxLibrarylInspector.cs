using UnityEngine;
using UnityEditor;
using System;

namespace SocialPoint.GrayboxLibrary
{
    [CustomEditor(typeof(GrayboxLibraryInspectorDummy))]
    public class GrayboxLibraryInspector : UnityEditor.Editor
    {
        private string[] tags = null;
        private GrayboxLibraryWebDrawer webDrawer;
        private GrayboxLibraryWebWindow webWindow;
        private EditorWindow inspectorWindow;

        public override void OnInspectorGUI()
        {
            if (EditorApplication.isCompiling)
                ClearView();

            else if (GrayboxLibraryWindow.assetChosen != null)
            {
                if (inspectorWindow == null)
                {
                    var editorAsm = typeof(UnityEditor.Editor).Assembly;
                    Type inspWndType = editorAsm.GetType("UnityEditor.InspectorWindow");
                    inspectorWindow = EditorWindow.GetWindow(inspWndType);
                    GrayboxLibraryWindow.window.Focus();
                }

                GrayboxAsset asset = GrayboxLibraryWindow.assetChosen;

                EditorGUILayout.LabelField(asset.name, EditorStyles.boldLabel);
                EditorGUILayout.Separator();

                GUILayout.BeginVertical();
                Rect previewRect;
                if (asset.animatedThumbnailPath.Length > 0)
                {
                    float width = Mathf.Min(inspectorWindow.position.width - 25, GrayboxLibraryWindow.animatedThumbWidth);
                    previewRect = GUILayoutUtility.GetRect(width, GrayboxLibraryWindow.animatedThumbHeight, GUILayout.Width(width));
                    if (webDrawer == null)
                    {
                        webWindow = GrayboxLibraryWebWindow.Launch();
                        Rect webWindowRect = new Rect(previewRect.x + inspectorWindow.position.x, previewRect.y + inspectorWindow.position.y, previewRect.width, previewRect.height);
                        Rect webDrawerRect = new Rect(0, 0, webWindowRect.width, webWindowRect.height);
                        webDrawer = new GrayboxLibraryWebDrawer(webWindow, asset.animatedThumbnailPath, webDrawerRect);
                    }
                    if (previewRect.width > 1)
                    {
                        Rect webWindowRect = new Rect(previewRect.x + inspectorWindow.position.x, previewRect.y + inspectorWindow.position.y, previewRect.width, previewRect.height);
                        webWindow.Draw(webWindowRect);
                        Rect webDrawerRect = new Rect(0, 0, webWindowRect.width, webWindowRect.height);
                        webDrawer.Draw(webDrawerRect);
                    }
                    GUILayout.Label("", GUILayout.Height(10));

                    GUILayout.Label("", GUILayout.Height(10));
                }
                else
                {
                    previewRect = GUILayoutUtility.GetRect(inspectorWindow.position.width, inspectorWindow.position.width * (GrayboxLibraryWindow.thumbHeight / GrayboxLibraryWindow.thumbWidth));
                    GUI.DrawTexture(previewRect, asset.thumbnail);
                }


                GUILayout.Label("", GUILayout.Height(10));

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                GUILayout.Label(asset.name);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                if (tags == null)
                    tags = GrayboxLibraryWindow.tool.GetAssetTagsAsText(asset);

                foreach (string tag in tags)
                {
                    GUILayout.Label(tag);
                }
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
                GUILayout.Label("", GUILayout.Height(20));
                if (GUILayout.Button("Add to Scene", GUILayout.Width(100f), GUILayout.Height(25f)))
                    GrayboxLibraryWindow.InstantiateAsset();

                GUILayout.EndVertical();
            }
            else
            {
                ClearView();

                DrawDefaultInspector();
            }
        }

        void ClearView()
        {
            if (webDrawer != null)
                webDrawer.ClearView();
            if (webWindow != null)
                webWindow.Close();
        }

        void OnDestroy()
        {
            ClearView();
        }
    }
}