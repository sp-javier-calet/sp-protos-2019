using UnityEngine;
using UnityEditor;
using System.Text;

namespace SpartaTools.Editor.Build
{
    public class BuildReport
    {
        readonly StringBuilder _builder = new StringBuilder("Sparta Build Report");
        int CurrentIndent;

        BuildReport Indent()
        {
            CurrentIndent++;
            return this;
        }

        BuildReport IndentBack()
        {
            CurrentIndent--;
            return this;
        }

        BuildReport AddTitle(string title)
        {
            return Add("# " + title); 
        }

        BuildReport Add(string label, string content)
        {
            return Add(string.Format("{0}: {1}", label, content));
        }

        BuildReport Add(string line)
        {
            for(int i = 0; i < CurrentIndent; i++)
            {
                _builder.Append("  ");
            }

            _builder.AppendLine(line);
            return this;
        }

        public BuildReport CollectPlayerSettings()
        {
            AddTitle("Player Settings")
            .Indent()
                .AddTitle("Application")
                .Indent()
                    .Add("Product Name", PlayerSettings.productName)
                    .Add("Version", PlayerSettings.bundleVersion)
                    .Add("Bundle id", PlayerSettings.bundleIdentifier)
                    .Add("Development build", EditorUserBuildSettings.development.ToString())
                .IndentBack()

                .AddTitle("Android")
                .Indent()
                    .Add("Icon", PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Android)[0].name)
                    .Add("Bundle Version Code", PlayerSettings.Android.bundleVersionCode.ToString())
                    .Add("Flags", PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android))
                    .Add("Keystore path", PlayerSettings.Android.keystoreName)
                    .Add("Keystore pass", PlayerSettings.Android.keystorePass)
                    .Add("Keystore alias", PlayerSettings.Android.keyaliasName)
                    .Add("Keystore alias pass", PlayerSettings.Android.keyaliasPass)
                .IndentBack()

                .AddTitle("iOS")
                .Indent()
                    .Add("Icon", PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.iOS)[0].name)
                    .Add("Flags", PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS))
                    .AddTitle("Xcodemods")
                    .Indent()
                        .Add("Stored Schemes", EditorPrefs.GetString(BuildSet.XcodeModSchemesPrefsKey))
                    .IndentBack()
                .IndentBack()
            .IndentBack();

            return this;
        }

        public void Dump()
        {
            Debug.Log(_builder.ToString());
        }
    }
}
