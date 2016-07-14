using UnityEngine;
using UnityEditor;
using System.Text;

namespace SpartaTools.Editor.Build
{
    public class BuildReport
    {
        readonly StringBuilder _builder = new StringBuilder("## Sparta Build Report ## \n");
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

        BuildReport Space()
        {
            _builder.AppendLine();
            return this;
        }

        public BuildReport AddBuildSetInfo(BuildSet bs)
        {
            string error;
            AddTitle("Build Set")
                .Indent()
                    .Add("Name", bs.Name)
                    .Add("FileName", bs.name)
                    .Add("Is default config", bs.IsDefaultConfig.ToString())
                    .Add("Is Valid", bs.IsValid(out error).ToString())
                    .Add("Validation error", error)

                    .AddTitle("App")
                    .Indent()
                        .Add("Product Name", bs.App.ProductName)
                        .Add("Override Icon", bs.App.OverrideIcon.ToString())
                        .Add("Icon", bs.App.IconTexture.ToString())
                    .IndentBack()

                    .AddTitle("Common")
                    .Indent()
                        .Add("Flags", bs.Common.Flags)
                        .Add("Rebuild Native Plugins", bs.Common.RebuildNativePlugins.ToString())
                        .Add("Is Development Build", bs.Common.IsDevelopmentBuild.ToString())
                        .Add("Include Debug Scenes", bs.Common.IncludeDebugScenes.ToString())
                    .IndentBack()

                    .AddTitle("Ios")
                    .Indent()
                        .Add("Bundle Identifier", bs.Ios.BundleIdentifier)
                        .Add("Flags", bs.Ios.Flags)
                        .Add("XcodeMod Schemes", bs.Ios.XcodeModSchemes)
                        .Add("Use Global Provisioning", bs.Ios.UseEnvironmentProvisioningUuid.ToString())
                        .Add("Removed resources", bs.Ios.RemovedResources)
                    .IndentBack()

                    .AddTitle("Android")
                    .Indent()
                        .Add("Bundle Identifier", bs.Android.BundleIdentifier)
                        .Add("Force Bundle Version Code", bs.Android.ForceBundleVersionCode.ToString())
                        .Add("Bundle Version Code", bs.Android.BundleVersionCode.ToString())
                        .Add("Flags", bs.Android.Flags)
                        .Add("Removed resources", bs.Android.RemovedResources)
                        .Add("Use keystore", bs.Android.UseKeystore.ToString())
                        .AddTitle("Keystore")
                        .Indent()
                            .Add("Path", bs.Android.Keystore.Path)
                            .Add("File Password", bs.Android.Keystore.FilePassword)
                            .Add("Alias", bs.Android.Keystore.Alias)
                            .Add("Password", bs.Android.Keystore.Password)
                        .IndentBack()
                    .IndentBack()
                .IndentBack()
                .Space();
            return this;
        }

        public BuildReport CollectBaseSettings()
        {
            AddTitle("Base Settings")
            .Indent()
                .AddBuildSetInfo(BaseSettings.Load())
            .IndentBack()
            .Space();
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
                    .AddTitle("Global Provisioning")
                    .Indent()
                        .Add("Stored Provisioning", EditorPrefs.GetString(BuildSet.ProvisioningProfilePrefsKey))
                    .IndentBack()
                .IndentBack()
            .IndentBack()
            .Space();

            return this;
        }

        public void Dump()
        {
            Debug.Log(_builder.ToString());
        }
    }
}
