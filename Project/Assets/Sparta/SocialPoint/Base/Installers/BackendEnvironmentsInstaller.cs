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
        [HideInInspector]
        public class DefaultEnvironmentsData
        {
            public string ProductionEnvironment = DefaultProductionName;
            public string DefaultEnvironment = DefaultDevelopmentName;
        }

        public SettingsData Settings = new SettingsData();
        public DefaultEnvironmentsData Defaults = new DefaultEnvironmentsData();

        public override void InstallBindings()
        {
            Container.Bind<BackendEnvironment>().ToMethod<BackendEnvironment>(CreateEnvironments);
            Container.Bind<IBackendEnvironment>().ToLookup<BackendEnvironment>();
        }

        BackendEnvironment CreateEnvironments()
        {
            return new BackendEnvironment(
                Settings.Environments, 
                Defaults.ProductionEnvironment, 
                Defaults.DefaultEnvironment
            );
        }
    }
}