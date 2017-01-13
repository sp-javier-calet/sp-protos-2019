using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Base
{
    public enum EnvironmentType
    {
        Production,
        PreProduction,
        QA,
        Development
    }

    [Serializable]
    public struct Environment
    {
        public string Name;
        public string Url;
        public EnvironmentType Type;
    }

    public class BackendEnvironment
    {
        public readonly Environment[] Environments;
        readonly Dictionary<string, Environment> _map;
        readonly Environment _defaultEnvironment;
        readonly Environment _productionEnvironment;

        public BackendEnvironment(Environment[] envs, string defaultProduction, string defaultDevelopment)
        {
            Environments = envs;
            _map = new Dictionary<string, Environment>();
            for(var i = 0; i < Environments.Length; ++i)
            {
                var env = Environments[i];
                _map.Add(env.Name, env);

                if(env.Type == EnvironmentType.Development && env.Name == defaultDevelopment)
                {
                    _defaultEnvironment = env;
                }
                else if(env.Type == EnvironmentType.Production && env.Name == defaultProduction)
                {
                    _productionEnvironment = env;
                }
            }

            DebugUtils.Assert(!string.IsNullOrEmpty(_defaultEnvironment.Url), "No Default Development environment");
            DebugUtils.Assert(!string.IsNullOrEmpty(_productionEnvironment.Url), "No Default Production environment");
        }

        Environment CurrentEnvironment
        {
            get
            {
                var environmentUrl = EnvironmentSettings.Instance.EnvironmentUrl;
                if(!string.IsNullOrEmpty(environmentUrl))
                {
                    var forcedEnv = new Environment { 
                        Name = "Forced", 
                        Url = environmentUrl, 
                        Type = EnvironmentType.Production
                    };
                    return forcedEnv;
                }
                return DebugUtils.IsDebugBuild ? _defaultEnvironment : _productionEnvironment;
            }
        }

        public string GetUrl()
        {
            return CurrentEnvironment.Url;
        }

        public string GetUrl(string name)
        {
            var env = GetEnvironment(name);
            return env.HasValue ? env.Value.Url : null;
        }

        public Environment GetEnvironment()
        {
            return CurrentEnvironment;
        }

        public Environment? GetEnvironment(string name)
        {
            Environment env;
            if(_map.TryGetValue(name, out env))
            {
                return env;
            }
            return null;
        }
    }
}