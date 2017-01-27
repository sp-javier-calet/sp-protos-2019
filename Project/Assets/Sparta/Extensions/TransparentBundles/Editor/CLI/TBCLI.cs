using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;
using System.IO;
using UnityEditor;
using System.Reflection;

namespace SocialPoint.TransparentBundles
{
    public class TBCLI
    {
        #region IO_Classes
        public class OutputCLI
        {
            public bool success = false;
            public List<string> log = new List<string>();

            public void Save(string path, bool pretty = true)
            {
                JsonWriter writer = new JsonWriter();
                writer.PrettyPrint = pretty;
                JsonMapper.ToJson(this, writer);
                File.WriteAllText(path, writer.ToString());
            }
        }

        public class InputCLI
        {
            public string MethodName;

            public static InputCLI Load(string path)
            {
                return JsonMapper.ToObject<InputCLI>(File.ReadAllText(path));
            }
        }

        public class CalculateBundlesInput : InputCLI
        {
            public List<DependencySystem.BundleInfo> ManualBundles;
        }

        public class CalculateBundlesOutput : OutputCLI
        {
            public Dictionary<string, BundleDependenciesData> CurrentBundles;
        }

        public class BuildBundlesInput : InputCLI
        {
            public MobileTextureSubtarget TextureFormat = MobileTextureSubtarget.Generic;
            public string BundlesPath;
        }

        public class BuildBundlesOutput : OutputCLI
        {
            public string[] bundles;
        }
        #endregion

        private const string _inputJson = "-input-json";
        private const string _outputJson = "-output-json";

        #region CLI_Methods
        public static void Run()
        {
            OutputCLI output = new OutputCLI();
            var outputPath = string.Empty;
            try
            {
                var arguments = new List<string>(Environment.GetCommandLineArgs());
                var inputs = InputCLI.Load(GetArgument(arguments, _inputJson));
                outputPath = GetArgument(arguments, _outputJson);

                output = (OutputCLI) typeof(TBCLI).GetMethod(inputs.MethodName).Invoke(null, new object[] { inputs });
            }
            catch(Exception e)
            {
                string msg = "CLI RUN ERROR - " + e;
                output.log.Add(msg);
                Debug.LogError(msg);
            }


            if(outputPath != string.Empty)
            {
                // Output Saving
                output.Save(outputPath);
            }
        }


        public static OutputCLI CalculateBundles(CalculateBundlesInput input)
        {
            OutputCLI results = new CalculateBundlesOutput();
            DependencySystem.OnLogMessage += (x, y) => results.log.Add(y.ToString() + " - " + x);

            DependencySystem.UpdateManifest(input.ManualBundles);

            ((CalculateBundlesOutput)results).CurrentBundles = DependencySystem.GetManifest();

            results.success = true;
            results.log.Add("OK - Process completed");

            return results;
            
        }

        public static OutputCLI BuildBundles(BuildBundlesInput input)
        {                      
            EditorUserBuildSettings.androidBuildSubtarget = input.TextureFormat;

            var results = new BuildBundlesOutput();
            DependencySystem.OnLogMessage += (x, y) => results.log.Add(y.ToString() + " - " + x);

            DependencySystem.ValidateAllBundles();
            var manifest = BuildPipeline.BuildAssetBundles(input.BundlesPath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.AppendHashToAssetBundleName, EditorUserBuildSettings.activeBuildTarget);

            ((BuildBundlesOutput)results).bundles = manifest.GetAllAssetBundles();

            results.success = true;
            results.log.Add("OK - Process completed");

            return results;
        }

        #endregion

        #region Helpers
        private static string GetArgument(List<string> arguments, string argument)
        {
            var targetIdx = arguments.IndexOf(argument);
            if(targetIdx < 0)
            {
                throw new ArgumentException("There is no " + argument + " argument");
            }

            return arguments[targetIdx + 1];
        }

        private static bool GetOptionalArgument(List<string> arguments, string argument, out string value)
        {
            var targetIdx = arguments.IndexOf(argument);
            if(targetIdx < 0)
            {
                value = null;
                return false;
            }
            value = arguments[targetIdx + 1];
            return true;
        }
        #endregion
    }
}
