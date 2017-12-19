using System.IO;
using UnityEngine;
using UnityEditor;

namespace SpartaTools.Editor.View
{
    public static class EditorToolsWindow
    {
        [MenuItem("Sparta/Editor/Restart &#r", false, 1000)]
        public static void RestartEditor()
        {
            Restart();   
        }

        static void Restart()
        {
            EditorApplication.OpenProject(ProjectRoot);
        }

        static string ProjectRoot
        {
            get
            {
                return Path.Combine(Application.dataPath, "..");
            }
        }
    }
}
