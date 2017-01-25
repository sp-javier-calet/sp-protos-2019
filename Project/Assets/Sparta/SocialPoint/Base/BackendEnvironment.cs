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
        readonly Environment _defaultEnvironment;

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

            bool defaultAssigned = false;
            var selected = storage.Selected;
            if(!string.IsNullOrEmpty(selected))
            {
                defaultAssigned = _map.TryGetValue(selected, out _defaultEnvironment);
            }

            var defaultEnv = storage.Default;
            if(!defaultAssigned && !string.IsNullOrEmpty(defaultEnv))
            {
                defaultAssigned = _map.TryGetValue(defaultEnv, out _defaultEnvironment);
            }

            DebugUtils.Assert(defaultAssigned, "No Default Development assigned");
            DebugUtils.Assert(!string.IsNullOrEmpty(_defaultEnvironment.Url), "No Default Development environment");
        }

        public IBackendEnvironmentStorage Storage
        {
            get
            {
                return _storage;
            }
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
                        Type = EnvironmentType.Production
                    };
                    return forcedEnv;
                }
                return null;
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
                
                return _defaultEnvironment;
            }
        }

        #region IBackendEnvironment implementation

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