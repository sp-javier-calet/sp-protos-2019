using UnityEditor;
using UnityEngine;
using SocialPoint.Base;

namespace SocialPoint.Exporter
{
    public class ProjectExportersOutputView : EditorWindow, Log.ILogger
    {
        Vector2 _scrollPos;
        string _text;

        public void Log(string message)
        {
            _text += string.Format("<color=green><b>{0}</b></color>\n", message);
        }

        public void LogWarning(string message)
        {
            _text += string.Format("<color=yellow><b>{0}</b></color>\n", message);
        }

        public void LogError(string message)
        {
            _text += string.Format("<color=red><b>{0}</b></color>\n", message);
        }

        public void LogException(System.Exception e)
        {
            _text += string.Format("<color=red><b>Unhandled {0}: {1}</b><br/>\n{2}</color>\n", e.GetType(), e.Message, e.StackTrace);
        }

        public static ProjectExportersOutputView Show(string text)
        {
            ProjectExportersOutputView window = (ProjectExportersOutputView)EditorWindow.GetWindow(typeof(ProjectExportersOutputView), true, "Export Output");
            window.minSize = new Vector2(400, 300);
            window._text = text;
            window.Show();
            return window;
        }

        void OnGUI()
        {
            float width = this.position.width;
            float height = this.position.height;

            GUI.contentColor = Color.white;
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Width(width), GUILayout.Height(height));
            {
                var style = new GUIStyle() { wordWrap = true };
                style.normal.textColor = Color.white;
                EditorGUILayout.SelectableLabel(_text, style, GUILayout.Width(width - 20));
            }
            GUILayout.EndScrollView();
        }
    }
}