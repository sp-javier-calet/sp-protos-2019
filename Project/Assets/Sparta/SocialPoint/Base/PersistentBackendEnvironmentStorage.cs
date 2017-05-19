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

                #if ADMIN_PANEL
                return _default;
                #endif

                return DebugUtils.IsDebugBuild ? _default : _production;
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

                #if ADMIN_PANEL
                return stored;
                #endif

                return DebugUtils.IsDebugBuild ? stored : null;
            }
        }
    }
}