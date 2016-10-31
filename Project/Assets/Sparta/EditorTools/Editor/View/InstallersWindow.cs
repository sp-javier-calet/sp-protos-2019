using UnityEditor;
using SpartaTools.Editor.Utils;

namespace SpartaTools.Editor.View
{
    public static class InstallersWindow
    {
        [MenuItem("Sparta/Installers/Refresh", false, 500)]
        public static void RefreshInstallers()
        {
            var inspector = new AssemblyInspector();
            var t = inspector.TypeByName("IScriptableInstaller");
            var installers = inspector.WichImplements(t);
            inspector.Invoke(installers, "Create");

            var installer = inspector.TypeByName("HttpClientScriptInstaller");
            inspector.Invoke(installer, "Create");
        }
    }
}