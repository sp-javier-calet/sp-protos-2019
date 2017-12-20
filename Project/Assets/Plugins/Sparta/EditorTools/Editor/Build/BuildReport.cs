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
            return Add(string.Format("{0}: {1}", label, content ?? "<null>"));
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
                        .Add("Product Version", bs.App.Version)
                        .Add("Override Build Number", bs.App.OverrideBuild.ToString())
                        .Add("Build Number", bs.App.BuildNumber.ToString())
                        .Add("Override Icon", bs.App.OverrideIcon.ToString())
                        .Add("Icon", bs.App.IconTexture.ToString())
                        .Add("Jenkins Forced Environment Url", BuildSet.EnvironmentUrl)
                    .IndentBack()

                    .AddTitle("Common")
                    .Indent()
                        .Add("Flags", bs.Common.Flags)
                        .Add("Rebuild Native Plugins", bs.Common.RebuildNativePlugins.ToString())
                        .Add("Is Development Build", bs.Common.IsDevelopmentBuild.ToString())
                        .Add("Append Build", bs.Common.AppendBuild.ToString())
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

        static string GetIconName(BuildTargetGroup targetGroup)
        {
            var textures = PlayerSettings.GetIconsForTargetGroup(targetGroup);
            return (textures.Length > 0 && textures[0] != null) ? textures[0].name : "<null>";
        }

        public BuildReport CollectPlayerSettings()
        {
            AddTitle("Player Settings")
            .Indent()
                .AddTitle("Application")
                .Indent()
                    .Add("Product Name", PlayerSettings.productName)
                    .Add("Version", PlayerSettings.bundleVersion)
                #if UNITY_2017
                    .Add("Bundle id", PlayerSettings.applicationIdentifier)
                #else
                    .Add("Bundle id", PlayerSettings.bundleIdentifier)
                #endif
                    .Add("Development build", EditorUserBuildSettings.development.ToString())
                .IndentBack()

                .AddTitle("Android")
                .Indent()
                    .Add("Icon", GetIconName(BuildTargetGroup.Android))
                    .Add("Bundle Version Code", PlayerSettings.Android.bundleVersionCode.ToString())
                    .Add("Flags", PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android))
                    .Add("Keystore path", PlayerSettings.Android.keystoreName)
                    .Add("Keystore pass", PlayerSettings.Android.keystorePass)
                    .Add("Keystore alias", PlayerSettings.Android.keyaliasName)
                    .Add("Keystore alias pass", PlayerSettings.Android.keyaliasPass)
                .IndentBack()

                .AddTitle("iOS")
                .Indent()
                    .Add("Icon", GetIconName(BuildTargetGroup.iOS))
                    .Add("Flags", PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS))
                    .AddTitle("Xcodemods")
                    .Indent()
                        .Add("Stored Schemes", BuildSet.CurrentXcodeModSchemes)
                    .IndentBack()
                    .AddTitle("Global Provisioning")
                    .Indent()
                        .Add("Environment Provisioning", BuildSet.EnvironmentProvisioningUuid)
                        .Add("Stored Provisioning", BuildSet.CurrentGlobalProvisioningUuid)
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
