using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;
using System.IO;
using UnityEditor;
using System.Reflection;
using System.Linq;

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
            public static InputCLI Load(string path, string type)
            {
                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                Type currentType = currentAssembly.GetTypes().SingleOrDefault(t => t.Name == type);

                if(currentType == null)
                {
                    currentType = typeof(InputCLI);
                }
                return (InputCLI) TBUtils.GetJsonMapperToObjGeneric(currentType).Invoke(null, new object[] { File.ReadAllText(path) });
            }

            public void Save(string path, bool pretty = true)
            {
                JsonWriter writer = new JsonWriter();
                writer.PrettyPrint = pretty;
                JsonMapper.ToJson(this, writer);
                File.WriteAllText(path, writer.ToString());
            }
        }

        public class CalculateBundlesInput : InputCLI
        {
            public List<DependencySystem.BundleInfo> ManualBundles;
        }

        public class CalculateBundlesOutput : OutputCLI
        {
            public Dictionary<string, BundleDependenciesData> BundlesDictionary;
        }

        public class BuildBundlesInput : InputCLI
        {
            public Dictionary<string, BundleDependenciesData> BundlesDictionary;
            public string TextureFormat = "Generic";
            public string BundlesPath;
        }

        public class BuildBundlesOutput : OutputCLI
        {
            public string[] bundles;
            public List<string> BuildLog;
        }
        #endregion

        private const string _inputJson = "-input-json";
        private const string _outputJson = "-output-json";
        private const string _methodName = "-method-name";
        
        #region CLI_Methods
        public static void Run()
        {
            OutputCLI output = new OutputCLI();
            var outputPath = string.Empty;
            object[] args = null;
            try
            {
                var arguments = new List<string>(Environment.GetCommandLineArgs());
                var jsonPath = GetArgument(arguments, _inputJson);
                var methodName = GetArgument(arguments, _methodName);
                outputPath = GetArgument(arguments, _outputJson);

                InputCLI inputs = InputCLI.Load(jsonPath, methodName + "Input");

                args = new object[] { inputs, output, null };
                typeof(TBCLI).GetMethod(methodName).Invoke(null, args);

                output.success = true;
                output.log.Add("OK - Process completed");
            }
            catch(Exception e)
            {
                if(args != null)
                {
                    output = (OutputCLI)args[2];
                }
                output.success = false;
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

        public static void CalculateBundles(CalculateBundlesInput input, ref OutputCLI output)
        {
            output = new CalculateBundlesOutput();
            var typedOutput = (CalculateBundlesOutput)output;

            DependencySystem.OnLogMessage += (x, y) => typedOutput.log.Add(y.ToString() + " - " + x);

            DependencySystem.UpdateManifest(input.ManualBundles);

            typedOutput.BundlesDictionary = DependencySystem.Manifest.GetDictionary();            
        }

        public static void BuildBundles(BuildBundlesInput input, ref OutputCLI output)
        {
            output = new BuildBundlesOutput();
            var typedOutput = (BuildBundlesOutput)output;

            EditorUserBuildSettings.androidBuildSubtarget = (MobileTextureSubtarget)Enum.Parse(typeof(MobileTextureSubtarget), input.TextureFormat);
            
            DependencySystem.OnLogMessage += (x, y) => typedOutput.log.Add(y.ToString() + " - " + x);

            DependencySystem.PrepareForBuild(input.BundlesDictionary);

            Application.LogCallback Callback = (msg, stack, type) =>
            {
                if(type == LogType.Error || type == LogType.Exception || type == LogType.Warning)
                {
                    typedOutput.BuildLog.Add(type + " - " + msg + "\n" + stack);
                }
            };

            Application.logMessageReceived += Callback;
            var manifest = BuildPipeline.BuildAssetBundles(input.BundlesPath, BuildAssetBundleOptions.DeterministicAssetBundle, EditorUserBuildSettings.activeBuildTarget);
            Application.logMessageReceived -= Callback;
            if(manifest != null)
            {
                typedOutput.bundles = manifest.GetAllAssetBundles();
            }
            else
            {
                throw new Exception("Error during building process");
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
