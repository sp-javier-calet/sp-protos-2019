using UnityEngine;
using UnityEditor;

namespace SpartaTools.Editor.View
{
    public interface ISubWindow
    {
        void OnGUI();
    }

    public class ComposedWindow : EditorWindow
    {
        protected ISubWindow[] Views;

        Vector2 _scrollPosition;

        void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            if(Views != null)
            {
                foreach(var view in Views)
                {
                    EditorGUILayout.Space();
                    view.OnGUI();
                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
