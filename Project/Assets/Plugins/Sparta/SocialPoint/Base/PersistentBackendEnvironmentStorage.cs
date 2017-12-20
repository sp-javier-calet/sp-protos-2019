using UnityEngine;

namespace SocialPoint.Base
{
    public sealed class PersistentBackendEnvironmentStorage : IBackendEnvironmentStorage
    {
        const string SelectedBackendEnvPrefsKey = "sparta_selected_backend_environment";

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
                var value = DebugUtils.IsDebugBuild ? _default : _production;
                #if ADMIN_PANEL
                value = _default;
                #endif
                return value;
            }
        }

        // Environment selection should be disabled in production builds
        public string Selected
        {
            set
            {

                #if ADMIN_PANEL
                if(string.IsNullOrEmpty(value))
                {
                    PlayerPrefs.DeleteKey(SelectedBackendEnvPrefsKey);
                }
                else
                {
                    PlayerPrefs.SetString(SelectedBackendEnvPrefsKey, value);
                }
                #else
                if(DebugUtils.IsDebugBuild)
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
                #endif
            }
            get
            {
                var stored = PlayerPrefs.GetString(SelectedBackendEnvPrefsKey);
                var value = DebugUtils.IsDebugBuild ? stored : null;
                #if ADMIN_PANEL
                value = stored;
                #endif
                return value;
            }
        }
    }
}
