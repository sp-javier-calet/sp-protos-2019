using UnityEngine;
using UnityEditor;
using System.IO;
using SpartaTools.Editor.Build;

namespace SpartaTools.Editor.View
{
    public sealed class SettingsWindow : ComposedWindow
    {
        [MenuItem("Sparta/Settings...", false, 300)]
        public static void OpenProxySettings()
        {
            EditorWindow.GetWindow(typeof(SettingsWindow), false, "Settings", true);
        }

        public SettingsWindow()
        {
            Views = new ISubWindow[] { new CompilerSettingsWindow(), new ProxySettingsWindow() };
        }

        protected void OnFocus()
        {
            Sparta.SetIcon(this, "Settings", "Sparta Editor Settings");
        }

    }
}
