using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace SocialPoint.Base
{
    public class UBAutoBuilderConfigurator : IAutoBuilderConfigurator
    {
        [AutoBuilderConfiguratorFactory]
        public static IAutoBuilderConfigurator CreateUBAutoBuilderConfigurator(AutoBuilderConfiguration configuration)
        {
            return new UBAutoBuilderConfigurator();
        }

        public AutoBuilderConfiguration Configure(AutoBuilderConfiguration baseConfiguration)
        {

            #if UNITY_ANDROID
            baseConfiguration.AndroidKeyStoreName = "Assets/Plugins/Android/socialpoint.keystore";
            baseConfiguration.AndroidKeyStorePass = "android";
            baseConfiguration.AndroidKeyAliasName = "android";
            baseConfiguration.AndroidKeyAliasPass = "android";
            baseConfiguration.BundleIdentifier = "es.socialpoint.basegame";
            #endif

            return baseConfiguration;
        }

        public void OnBuildCompleted(AutoBuilderConfiguration configuration, string result)
        {
        }
    }
}