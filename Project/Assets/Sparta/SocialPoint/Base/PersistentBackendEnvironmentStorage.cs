using UnityEngine;

namespace SocialPoint.Base
{
    public sealed class PersistentBackendEnvironmentStorage : IBackendEnvironmentStorage
    {
        const string SelectedBackendEnvPrefsKey = "selected_backend_environment";

        readonly string _default;
        readonly string _production;

        public PersistentBackendEnvironmentStorage(string productionEnvironment, string defaultEnvironment)
        {
            _default = defaultEnvironment;
            _production = productionEnvironment;
        }

        public string Default
        {
            get
            {
                return DebugUtils.IsDebugBuild ? _default : _production;
            }
        }

        public string Selected
        {
            set
            {
                if(string.IsNullOrEmpty(value))
                {
                    PlayerPrefs.DeleteKey(SelectedBackendEnvPrefsKey);
                }
                else
                {
                    PlayerPrefs.SetString(SelectedBackendEnvPrefsKey, value);
                }
            }
            get
            {
                return PlayerPrefs.GetString(SelectedBackendEnvPrefsKey);
            }
        }
    }
}