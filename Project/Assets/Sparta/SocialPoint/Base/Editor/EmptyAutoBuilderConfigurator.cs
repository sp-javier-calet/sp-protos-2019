using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace SocialPoint.Base
{
    public class EmptyAutoBuilderConfigurator : IAutoBuilderConfigurator
    {
        public AutoBuilderConfiguration Configure(AutoBuilderConfiguration baseConfiguration)
        {
            return baseConfiguration;
        }

        public void OnBuildCompleted(AutoBuilderConfiguration configuration, string result)
        {
        }
    }
}