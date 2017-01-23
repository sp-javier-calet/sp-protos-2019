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
        private const string _inputJson = "-input-json";
        private const string _outputJson = "-output-json";
        private const string _platform = "-platform";
        private const string _textureFormat = "-texture-format";
        private static readonly string bundlespath = Directory.GetDirectoryRoot(Application.dataPath);


        #region CalculateBundles
        public class CalculateBundlesInput
        {
            public List<string> ManualBundles;

            public static CalculateBundlesInput Load(string path)
            {
                return JsonMapper.ToObject<CalculateBundlesInput>(File.ReadAllText(path));
            }

            public void Save(string path, bool pretty = true)
            {
                JsonWriter writer = new JsonWriter();
                writer.PrettyPrint = pretty;
                JsonMapper.ToJson(this, writer);
                File.WriteAllText(path, writer.ToString());
            }
        }

        public class CalculateBundlesOutput
        {
            public Dictionary<string, BundleDependenciesData> CurrentBundles;

            public CalculateBundlesOutput(Dictionary<string, BundleDependenciesData> updatedBundles)
            {
                CurrentBundles = updatedBundles;
            }

            public void Save(string path, bool pretty = true)
            {
                JsonWriter writer = new JsonWriter();
                writer.PrettyPrint = pretty;
                JsonMapper.ToJson(this, writer);
                File.WriteAllText(path, writer.ToString());
            }
        }


        public static void CalculateBundles()
        {
            try
            {
                CalculateBundlesInput inputs;
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
                    inputs = CalculateBundlesInput.Load(Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "input.json"));
                    outputPath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "output.json");
                }


                // Operations
                DependencySystem.UpdateManifest(inputs.ManualBundles);


                // Output Saving
                CalculateBundlesOutput results = new CalculateBundlesOutput(DependencySystem.GetManifest());

                results.Save(outputPath);
            }
            catch(Exception e)
            {
                Debug.LogError("Exception has been found: " + e);
            }
        }
        #endregion

        #region BuildBundles

        public class BuildBundlesOutput
        {
            public BuildBundlesOutput()
            {
            }

            public void Save(string path, bool pretty = true)
            {
                JsonWriter writer = new JsonWriter();
                writer.PrettyPrint = pretty;
                JsonMapper.ToJson(this, writer);
                File.WriteAllText(path, writer.ToString());
            }
        }

        public static void BuildBundles()
        {
            try
            {
                // Arguments Parsing
                var arguments = new List<string>(Environment.GetCommandLineArgs());

                string textureFormat;

                var textureFormatSpecified = GetOptionalArgument(arguments, _textureFormat, out textureFormat);
                var platform = GetArgument(arguments, _platform);
                string outputPath = GetArgument(arguments, _outputJson);

                var targetPlatform = (BuildTarget)Enum.Parse(typeof(BuildTarget), platform);


                // Operations
                if(textureFormatSpecified)
                {
                    var textureFormatEnum = (MobileTextureSubtarget)Enum.Parse(typeof(MobileTextureSubtarget), textureFormat);
                    EditorUserBuildSettings.androidBuildSubtarget = textureFormatEnum;
                }

                DependencySystem.ValidateAllBundles();
                var manifest = BuildPipeline.BuildAssetBundles(bundlespath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.AppendHashToAssetBundleName, targetPlatform);


                // Output Saving
                BuildBundlesOutput results = new BuildBundlesOutput();

                results.Save(outputPath);
            }
            catch(Exception e)
            {
                Debug.LogError("Exception has been found: " + e);
            }
        }

        #endregion

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
    }
}
