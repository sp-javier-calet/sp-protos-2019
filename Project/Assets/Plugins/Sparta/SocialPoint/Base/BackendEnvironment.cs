using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Base
{
    public class BackendEnvironment : IBackendEnvironment
    {
        public readonly Environment[] _environments;

        readonly Dictionary<string, Environment> _map;
        readonly IBackendEnvironmentStorage _storage;

        public BackendEnvironment(Environment[] envs, IBackendEnvironmentStorage storage)
        {
            _environments = envs;
            _storage = storage;

            _map = new Dictionary<string, Environment>();
            for(var i = 0; i < Environments.Length; ++i)
            {
                var env = Environments[i];
                _map.Add(env.Name, env);
            }

            DebugUtils.Assert(!string.IsNullOrEmpty(DefaultEnvironment.Url), "No Default Development environment");
        }

        Environment? ForcedEnvironment
        {
            get
            {
                var environmentUrl = EnvironmentSettings.Instance.EnvironmentUrl;
                if(!string.IsNullOrEmpty(environmentUrl))
                {
                    var forcedEnv = new Environment { 
                        Name = "Forced", 
                        Url = environmentUrl, 
                        Type = EnvironmentType.Development
                    };
                    return forcedEnv;
                }
                return null;
            }
        }

        Environment DefaultEnvironment
        {
            get
            {
                var defaultEnvironment = new Environment();
                bool defaultAssigned = false;
                var selected = _storage.Selected;
                var defaultEnv = _storage.Default;

                if(!string.IsNullOrEmpty(selected))
                {
                    defaultAssigned = _map.TryGetValue(selected, out defaultEnvironment);
                }

                if(!defaultAssigned && !string.IsNullOrEmpty(defaultEnv))
                {
                    _map.TryGetValue(defaultEnv, out defaultEnvironment);
                }

                return defaultEnvironment;
            }
        }

        Environment CurrentEnvironment
        {
            get
            {
                var forced = ForcedEnvironment;
                if(forced.HasValue)
                {
                    return forced.Value;
                }
                
                return DefaultEnvironment;
            }
        }

        #region IBackendEnvironment implementation

        public IBackendEnvironmentStorage Storage
        {
            get
            {
                return _storage;
            }
        }

        public Environment[] Environments
        {
            get
            {
                return _environments;
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


        #endregion
    }
}