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
            public void Save(string path, bool pretty = true)
            {
                JsonWriter writer = new JsonWriter();
                writer.PrettyPrint = pretty;
                JsonMapper.ToJson(this, writer);
                File.WriteAllText(path, writer.ToString());
            }
        }

        public class CalculateBundlesOutput : OutputCLI
        {
            public Dictionary<string, BundleDependenciesData> CurrentBundles;
        }

        public class BuildBundlesOutput : OutputCLI
        {
            public string[] bundles;
        }
        #endregion

        private const string _inputJson = "-input-json";
        private const string _outputJson = "-output-json";
        private const string _platform = "-platform";
        private const string _textureFormat = "-texture-format";
        private const string _bundlesPath = "-bundles-path";
        private static readonly string _defaultOutputPath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "output.json");

        #region CLI_Methods
        public static void CalculateBundles()
        {
            OutputCLI results = new OutputCLI();
            string outputPath = _defaultOutputPath;
            try
            {
                CalculateBundlesInput inputs;
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

                results = new CalculateBundlesOutput();
                DependencySystem.OnLogMessage += (x, y) => results.log.Add(y.ToString() + " - " + x);

                // Operations
                DependencySystem.UpdateManifest(inputs.ManualBundles);

                ((CalculateBundlesOutput)results).CurrentBundles = DependencySystem.GetManifest();

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
                results.Save(outputPath);
            }
        }

        public static void BuildBundles()
        {
            OutputCLI results = new OutputCLI();
            string outputPath = _defaultOutputPath;

            try
            {
                // Arguments Parsing
                string textureFormat;
                bool textureFormatSpecified;
                string platform;
                string bundlesPath;

                try
                {
                    var arguments = new List<string>(Environment.GetCommandLineArgs());
                    textureFormatSpecified = GetOptionalArgument(arguments, _textureFormat, out textureFormat);
                    platform = GetArgument(arguments, _platform);
                    outputPath = GetArgument(arguments, _outputJson);
                    bundlesPath = GetArgument(arguments, _bundlesPath);
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

                var targetPlatform = (BuildTarget)Enum.Parse(typeof(BuildTarget), platform);

                // Operations
                if(textureFormatSpecified)
                {
                    var textureFormatEnum = (MobileTextureSubtarget)Enum.Parse(typeof(MobileTextureSubtarget), textureFormat);
                    EditorUserBuildSettings.androidBuildSubtarget = textureFormatEnum;
                }

                results = new BuildBundlesOutput();
                DependencySystem.OnLogMessage += (x, y) => results.log.Add(y.ToString() + " - " + x);

                DependencySystem.ValidateAllBundles();
                var manifest = BuildPipeline.BuildAssetBundles(bundlesPath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.AppendHashToAssetBundleName, targetPlatform);

                // Output Saving
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
