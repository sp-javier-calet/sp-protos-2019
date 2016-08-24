using UnityEditor;
using System;
using UnityEngine;

namespace SocialPoint.Base
{
    public class AutoBuilderConfiguration
    {
        public string[] Levels { get; set; }

        public string LocationPathName { get; set; }

        public BuildTarget Target { get; set; }

        public BuildOptions Options { get; set; }

        public string AndroidKeyStoreName { get; set; }

        public string AndroidKeyStorePass { get; set; }

        public string AndroidKeyAliasName { get; set; }

        public string AndroidKeyAliasPass { get; set; }

        public string BundleIdentifier { get; set; }
    }

    public class AutoBuilderConfiguratorFactory : Attribute
    {
    }

    public interface IAutoBuilderConfigurator
    {
        AutoBuilderConfiguration Configure(AutoBuilderConfiguration baseConfiguration);

        void OnBuildCompleted(AutoBuilderConfiguration configuration, string result);
    }

    public static class IAutoBuilderConfiguratorExtensions
    {
        public static void BuildWithConfigure(this IAutoBuilderConfigurator configurator, AutoBuilderConfiguration baseConfiguration)
        {
            baseConfiguration = configurator.Configure(baseConfiguration);
            configurator.Build(baseConfiguration);
        }

        public static void Build(this IAutoBuilderConfigurator configurator, AutoBuilderConfiguration baseConfiguration)
        {
            string result = BuildPipeline.BuildPlayer(baseConfiguration.Levels, baseConfiguration.LocationPathName, baseConfiguration.Target, baseConfiguration.Options);

            Debug.Log(string.Format("AutoBuilder - Player Build finished with result: '{0}'", result));
            if(!string.IsNullOrEmpty(result))
            {
                throw new Exception(result);
            }

            configurator.OnBuildCompleted(baseConfiguration, result);
        }
    }
}
