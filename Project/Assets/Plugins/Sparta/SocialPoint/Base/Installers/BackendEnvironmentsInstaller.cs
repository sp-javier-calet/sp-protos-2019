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
            public bool PersistsSelection;
            public Environment[] Environments = { 
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
            public string IosProductionEnvironment = DefaultProductionName;
            public string AndroidProductionEnvironment = DefaultProductionName;
            public string CommonProductionEnvironment = DefaultDevelopmentName;
            public string DefaultEnvironment = DefaultDevelopmentName;

            public string CurrentProductionEnvironment
            {
                get
                {
                    switch(Application.platform)
                    {
                    case RuntimePlatform.Android:
                        return AndroidProductionEnvironment;
                    case RuntimePlatform.IPhonePlayer:
                        return IosProductionEnvironment;
                    }
                    return CommonProductionEnvironment;
                }
            }
        }

        public SettingsData Settings = new SettingsData();

        [HideInInspector]
        public DefaultEnvironmentsData Defaults = new DefaultEnvironmentsData();

        public override void InstallBindings()
        {
            if(Settings.PersistsSelection)
            {
                Container.Bind<IBackendEnvironmentStorage>().ToMethod<PersistentBackendEnvironmentStorage>(CreatePersistentStorage);
            }
            else
            {
                Container.Bind<IBackendEnvironmentStorage>().ToMethod<DefaultBackendEnvironmentStorage>(CreateStorage);
            }

            Container.Bind<BackendEnvironment>().ToMethod<BackendEnvironment>(CreateEnvironments);
            Container.Bind<IBackendEnvironment>().ToLookup<BackendEnvironment>();
        }

        PersistentBackendEnvironmentStorage CreatePersistentStorage()
        {
            return new PersistentBackendEnvironmentStorage(
                Defaults.CurrentProductionEnvironment, 
                Defaults.DefaultEnvironment
            );
        }

        DefaultBackendEnvironmentStorage CreateStorage()
        {
            return new DefaultBackendEnvironmentStorage(
                Defaults.CurrentProductionEnvironment, 
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
    }
}