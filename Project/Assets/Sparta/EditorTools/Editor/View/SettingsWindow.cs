using UnityEditor;

namespace SpartaTools.Editor.View
{
    public sealed class SettingsWindow : ComposedWindow
    {
        [MenuItem("Sparta/Settings...", false, 700)]
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
