using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Base
{
    public enum EnvironmentType
    {
        Production,
        QA,
        Development,
        Default
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

        public BackendEnvironment(Environment[] envs)
        {
            Environments = envs;
            _map = new Dictionary<string, Environment>();
            for(var i = 0; i < Environments.Length; ++i)
            {
                var env = Environments[i];
                _map.Add(env.Name, env);

                if(env.Type == EnvironmentType.Default)
                {
                    _defaultEnvironment = env;
                }
                else if(env.Type == EnvironmentType.Production)
                {
                    _productionEnvironment = env;
                }
            }
        }

        string JenkinsForcedUrl
        {
            get
            {
                var environmentUrl = EnvironmentSettings.Instance.EnvironmentUrl;
                if(!string.IsNullOrEmpty(environmentUrl))
                {
                    return environmentUrl;
                }
                return DebugUtils.IsDebugBuild ? _defaultEnvironment.Url : _productionEnvironment.Url;
            }
        }

        public string GetUrl()
        {
            return JenkinsForcedUrl;
        }

        public string GetUrl(string name)
        {
            Environment env;
            if(_map.TryGetValue(name, out env))
            {
                return env.Url;
            }
            return null;
        }
    }
}