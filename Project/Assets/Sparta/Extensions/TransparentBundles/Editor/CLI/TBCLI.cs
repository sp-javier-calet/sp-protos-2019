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
        private const string _methodName = "-method-name";

        #region CLI_Methods
        public static void Run()
        {
            OutputCLI output = new OutputCLI();
            var outputPath = string.Empty;
            try
            {
                InputCLI inputs;
                //try
                //{
                var arguments = new List<string>(Environment.GetCommandLineArgs());
                var jsonPath = GetArgument(arguments, _inputJson);
                var methodName = GetArgument(arguments, _methodName);
                inputs = InputCLI.Load(jsonPath, methodName + "Input");
                outputPath = GetArgument(arguments, _outputJson);
                //}catch(Exception e)
                //{
                //    var jsonPath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "input.json");
                //    inputs = InputCLI.LoadCommon(jsonPath);
                //    inputs = InputCLI.Load(jsonPath, inputs.MethodName + "Input");
                //    outputPath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "output.json");
                //}                

                typeof(TBCLI).GetMethod(methodName).Invoke(null, new object[] { inputs });

                output.success = true;
                output.log.Add("OK - Process completed");
            }
            catch(Exception e)
            {
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

        public static OutputCLI CalculateBundles(CalculateBundlesInput input)
        {
            CalculateBundlesOutput results = new CalculateBundlesOutput();
            DependencySystem.OnLogMessage += (x, y) => results.log.Add(y.ToString() + " - " + x);

            DependencySystem.UpdateManifest(input.ManualBundles);

            results.CurrentBundles = DependencySystem.GetManifest();
            
            return results;
            
        }

        public static OutputCLI BuildBundles(BuildBundlesInput input)
        {                      
            EditorUserBuildSettings.androidBuildSubtarget = input.TextureFormat;

            var results = new BuildBundlesOutput();
            DependencySystem.OnLogMessage += (x, y) => results.log.Add(y.ToString() + " - " + x);

            DependencySystem.ValidateAllBundles();
            var manifest = BuildPipeline.BuildAssetBundles(input.BundlesPath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.AppendHashToAssetBundleName, EditorUserBuildSettings.activeBuildTarget);

            results.bundles = manifest.GetAllAssetBundles();

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
