using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;
using System.IO;
using UnityEditor;

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

        public class InputCLI<T>
        {
            public static T Load(string path)
            {
                return JsonMapper.ToObject<T>(File.ReadAllText(path));
            }
        }

        public class CalculateBundlesInput : InputCLI<CalculateBundlesInput>
        {
            public List<DependencySystem.BundleInfo> ManualBundles;
        }

        public class CalculateBundlesOutput : OutputCLI
        {
            public BundlesManifest BundlesManifest;
        }

        public class BuildBundlesInput : InputCLI<BuildBundlesInput>
        {
            public BundlesManifest BundlesManifest;
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
        public static void CalculateBundles()
        {
            CalculateBundlesInput inputs;
            OutputCLI results = new OutputCLI();
            string outputPath;

            // Arguments Parsing
            try
            {
                var arguments = new List<string>(Environment.GetCommandLineArgs());
                inputs = CalculateBundlesInput.Load(GetArgument(arguments, _inputJson));
                outputPath = GetArgument(arguments, _outputJson);
            }
            catch(Exception e)
            {
                ////**TESTING ONLY** Faking args
                //inputs = CalculateBundlesInput.Load(Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "input.json"));

                throw new ArgumentException(e.ToString());
            }

            // Operations
            try
            {
                results = new CalculateBundlesOutput();
                DependencySystem.OnLogMessage += (x, y) => results.log.Add(y.ToString() + " - " + x);

                DependencySystem.UpdateManifest(inputs.ManualBundles);

                ((CalculateBundlesOutput)results).BundlesManifest = DependencySystem.GetManifest();

                results.success = true;
                results.log.Add("OK - Process completed");
            }
            catch(Exception e)
            {
                string msg = "CLI RUN ERROR - " + e;
                results.log.Add(msg);
                Debug.LogError(msg);
            }
            finally
            {
                // Output Saving
                results.Save(outputPath);
            }
        }

        public static void BuildBundles()
        {
            BuildBundlesInput inputs;
            OutputCLI results = new OutputCLI();
            string outputPath;

            // Arguments Parsing
            try
            {
                var arguments = new List<string>(Environment.GetCommandLineArgs());
                inputs = BuildBundlesInput.Load(GetArgument(arguments, _inputJson));
                outputPath = GetArgument(arguments, _outputJson);
            }
            catch(Exception e)
            {
                //// **TESTING ONLY** Faking args
                //textureFormatSpecified = false;
                //platform = EditorUserBuildSettings.activeBuildTarget.ToString();
                //textureFormat = string.Empty;
                //bundlesPath = Directory.GetDirectoryRoot(Application.dataPath);
                //// **END TESTING**

                throw new ArgumentException(e.ToString());
            }

            // Operations
            try
            {
                EditorUserBuildSettings.androidBuildSubtarget = inputs.TextureFormat;

                results = new BuildBundlesOutput();
                DependencySystem.OnLogMessage += (x, y) => results.log.Add(y.ToString() + " - " + x);
                
                DependencySystem.PrepareForBuild(inputs.BundlesManifest);
                var manifest = BuildPipeline.BuildAssetBundles(inputs.BundlesPath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.AppendHashToAssetBundleName, EditorUserBuildSettings.activeBuildTarget);

                ((BuildBundlesOutput)results).bundles = manifest.GetAllAssetBundles();

                results.success = true;
                results.log.Add("OK - Process completed");
            }
            catch(Exception e)
            {
                string msg = "CLI RUN ERROR - " + e;
                results.log.Add(msg);
                Debug.LogError(msg);
            }
            finally
            {
                // Output Saving
                results.Save(outputPath);
            }
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
