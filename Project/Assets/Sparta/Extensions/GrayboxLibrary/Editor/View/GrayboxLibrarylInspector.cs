using UnityEngine;
using UnityEditor;
using System;

namespace SocialPoint.GrayboxLibrary
{
    [CustomEditor(typeof(GrayboxLibraryInspectorDummy))]
    public class GrayboxLibraryInspector : UnityEditor.Editor
    {
        private string[] _tags = null;
        private GrayboxLibraryWebDrawer _webDrawer;
        private GrayboxLibraryWebWindow _webWindow;
        private EditorWindow _inspectorWindow;

        public override void OnInspectorGUI()
        {
            if(EditorApplication.isCompiling)
                ClearView();
            else if(GrayboxLibraryWindow.AssetChosen != null)
            {
                if(_inspectorWindow == null)
                {
                    var editorAsm = typeof(UnityEditor.Editor).Assembly;
                    Type inspWndType = editorAsm.GetType("UnityEditor.InspectorWindow");
                    _inspectorWindow = EditorWindow.GetWindow(inspWndType);
                    GrayboxLibraryWindow.Window.Focus();
                }

                GrayboxAsset asset = GrayboxLibraryWindow.AssetChosen;

                EditorGUILayout.LabelField(asset.Name, EditorStyles.boldLabel);
                EditorGUILayout.Separator();

                GUILayout.BeginVertical();

                DrawPreview(asset);

                GUILayout.Label("", GUILayout.Height(10));

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                GUILayout.Label(asset.Name);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                if(_tags == null)
                    _tags = GrayboxLibraryWindow.Tool.GetAssetTagsAsText(asset);

                foreach(string tag in _tags)
                {
                    GUILayout.Label(tag);
                }
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
                GUILayout.Label("", GUILayout.Height(20));
                if(GUILayout.Button("Add to Scene", GUILayout.Width(100f), GUILayout.Height(25f)))
                    GrayboxLibraryWindow.InstantiateAsset();

                GUILayout.EndVertical();
            }
            else
            {
                ClearView();

                DrawDefaultInspector();
            }
        }

        void DrawPreview(GrayboxAsset asset)
        {
            Rect previewRect;

            //Web view for gifs
            if (asset.AnimatedThumbnailPath.Length > 0)
            {
                float width = Mathf.Min(_inspectorWindow.position.width - 25, GrayboxLibraryWindow.AnimatedThumbWidth);
                previewRect = GUILayoutUtility.GetRect(width, GrayboxLibraryWindow.AnimatedThumbHeight, GUILayout.Width(width));
                if (_webDrawer == null)
                {
                    _webWindow = GrayboxLibraryWebWindow.Launch();
                    Rect webWindowRect = new Rect(previewRect.x + _inspectorWindow.position.x, previewRect.y + _inspectorWindow.position.y, previewRect.width, previewRect.height);
                    Rect webDrawerRect = new Rect(0, 0, webWindowRect.width, webWindowRect.height);
                    _webDrawer = new GrayboxLibraryWebDrawer(_webWindow, asset.AnimatedThumbnailPath, webDrawerRect);
                }
                if (previewRect.width > 1)
                {
                    Rect webWindowRect = new Rect(previewRect.x + _inspectorWindow.position.x, previewRect.y + _inspectorWindow.position.y, previewRect.width, previewRect.height);
                    _webWindow.Draw(webWindowRect);
                    Rect webDrawerRect = new Rect(0, 0, webWindowRect.width, webWindowRect.height);
                    _webDrawer.Draw(webDrawerRect);
                }
                GUILayout.Label("", GUILayout.Height(20));
            }
            //Image view for static images
            else
            {
                previewRect = GUILayoutUtility.GetRect(_inspectorWindow.position.width, _inspectorWindow.position.width * (GrayboxLibraryWindow.ThumbHeight / GrayboxLibraryWindow.ThumbWidth));
                GUI.DrawTexture(previewRect, asset.Thumbnail);
            }
        }

        void ClearView()
        {
            if(_webDrawer != null)
                _webDrawer.ClearView();
            if(_webWindow != null)
                _webWindow.Close();
        }

        void OnDestroy()
        {
            ClearView();
        }
    }
}