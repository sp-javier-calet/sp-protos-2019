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
        /// <summary>
        /// Generic output for any CLI function (All outputs need to inherit this)
        /// </summary>
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

        /// <summary>
        /// Generic input for any CLI function (All input need to inherit this)
        /// </summary>
        public class InputCLI
        {
            /// <summary>
            /// Loads the input as the type provided
            /// </summary>
            /// <param name="path">path to the input json</param>
            /// <param name="type">Subclass of InputCLI type that is contained in the json. If type is not found, InputCLI will be used</param>
            /// <returns>Instance parsed from the json contained in InputCLI superclass</returns>
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

        /// <summary>
        /// Inputs for CalculateBundles CLI call
        /// </summary>
        public class CalculateBundlesInput : InputCLI
        {
            public List<DependencySystem.BundleInfo> ManualBundles;
        }

        /// <summary>
        /// Outputs for CalculateBundles CLI call
        /// </summary>
        public class CalculateBundlesOutput : OutputCLI
        {
            public Dictionary<string, BundleDependenciesData> BundlesDictionary;
        }

        /// <summary>
        /// Inputs for BuildBundles CLI call
        /// </summary>
        public class BuildBundlesInput : InputCLI
        {
            public Dictionary<string, BundleDependenciesData> BundlesDictionary;
            public string TextureFormat = "Generic";
            public string BundlesPath;
        }

        /// <summary>
        /// Outputs for BuildBundles CLI call
        /// </summary>
        public class BuildBundlesOutput : OutputCLI
        {
            public string[] bundles;
            public List<string> BuildLog = new List<string>();
        }
        #endregion

        private const string _inputJson = "-input-json";
        private const string _outputJson = "-output-json";
        private const string _methodName = "-method-name";
        
        #region CLI_Methods
        /// <summary>
        /// Common entry point for all CLI calls, the method name will come as an argument and will be called via reflection with the appropriate input
        /// Run will also write the results in the provided output path even if the CLI call fails.
        /// </summary>
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

                args = new object[] { inputs, output };
                typeof(TBCLI).GetMethod(methodName).Invoke(null, args);
                output = (OutputCLI)args[1];

                output.success = true;
                output.log.Add("OK - Process completed");
            }
            catch(Exception e)
            {
                if(args != null)
                {
                    output = (OutputCLI)args[1];
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

        /// <summary>
        /// Calculates the bundles and its dependencies using the DependencySystem
        /// </summary>
        /// <param name="input">Input for the CLI call</param>
        /// <param name="output">Output ref to write the results</param>
        public static void CalculateBundles(CalculateBundlesInput input, ref OutputCLI output)
        {
            output = new CalculateBundlesOutput();
            var typedOutput = (CalculateBundlesOutput)output;

            DependencySystem.OnLogMessage += (x, y) => typedOutput.log.Add(y.ToString() + " - " + x);

            DependencySystem.UpdateManifest(input.ManualBundles);

            typedOutput.BundlesDictionary = DependencySystem.Manifest.GetDictionary();            
        }

        /// <summary>
        /// Builds the bundles previously calculated by Calculate bundles
        /// </summary>
        /// <param name="input">Input for the CLI call</param>
        /// <param name="output">Output ref to write the results</param>
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
        /// <summary>
        /// Given a key, gets an argument from the command line, throws an exception if key is not found
        /// </summary>
        /// <param name="arguments">Whole list of arguments</param>
        /// <param name="argument">Key of the desired argument</param>
        /// <returns>Argument value</returns>
        private static string GetArgument(List<string> arguments, string argument)
        {
            var targetIdx = arguments.IndexOf(argument);
            if(targetIdx < 0)
            {
                throw new ArgumentException("There is no " + argument + " argument");
            }

            return arguments[targetIdx + 1];
        }

        /// <summary>
        /// Given a key, gets an argument from the command line, returns null if not found
        /// </summary>
        /// <param name="arguments">Whole list of arguments</param>
        /// <param name="argument">Key of the desired argument</param>
        /// <returns>Argument value if found, null if not found</returns>
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
