using System;
using UnityEngine;
using SocialPoint.Dependency;

namespace SocialPoint.Base
{
    public class BackendEnvironmentsInstaller : ServiceInstaller
    {
        const string DefaultProductionUrl = "https://int-sp-bootstrap-000a.vpc01.use1.laicosp.net/api/v3";
        const string DefaultDevelopmentUrl = "http://int-sp-bootstrap-000a.vpc01.use1.laicosp.net/api/v3";

        const string DefaultDevelopmentName = "Development";
        const string DefaultProductionName = "Production";

        [Serializable]
        public class SettingsData
        {
            public Environment[] Environments = new Environment[] { 
                new Environment { 
                    Name = DefaultDevelopmentName, 
                    Url = DefaultDevelopmentUrl, 
                    Type = EnvironmentType.Development
                },
                new Environment { 
                    Name = DefaultProductionName, 
                    Url = DefaultProductionUrl, 
                    Type = EnvironmentType.Production
                } 
            };
        }

        /// Uses custom editor to modify Defaults
        /// See BackendEnvironmentsInstallerEditor

        [Serializable]
        public class DefaultEnvironmentsData
        {
            public string ProductionEnvironment = DefaultProductionName;
            public string DefaultEnvironment = DefaultDevelopmentName;
        }

        public SettingsData Settings = new SettingsData();

        [HideInInspector]
        public DefaultEnvironmentsData Defaults = new DefaultEnvironmentsData();

        public override void InstallBindings()
        {
            Container.Bind<IBackendEnvironmentStorage>().ToMethod<DefaultBackendEnvironmentStorage>(CreateStorage);

            Container.Bind<BackendEnvironment>().ToMethod<BackendEnvironment>(CreateEnvironments);
            Container.Bind<IBackendEnvironment>().ToLookup<BackendEnvironment>();
        }

        DefaultBackendEnvironmentStorage CreateStorage()
        {
            return new DefaultBackendEnvironmentStorage(
                Defaults.ProductionEnvironment, 
                Defaults.DefaultEnvironment
            );
        }

        BackendEnvironment CreateEnvironments()
        {
            return new BackendEnvironment(
                Settings.Environments, 
                Container.Resolve<IBackendEnvironmentStorage>()
            );
        }

        class DefaultBackendEnvironmentStorage : IBackendEnvironmentStorage
        {
            const string SelectedBackendEnvPrefsKey = "selected_backend_environment";

            readonly string _default;
            readonly string _production;

            public DefaultBackendEnvironmentStorage(string productionEnvironment, string defaultEnvironment)
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
}