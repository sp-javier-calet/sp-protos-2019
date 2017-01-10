using System;
using SocialPoint.Dependency;

namespace SocialPoint.Base
{

    public class BackendEnvironmentsInstaller : ServiceInstaller
    {
        const string DefaultDevelopmentUrl = "http://int-sp-bootstrap-000a.vpc01.use1.laicosp.net/api/v3";

        [Serializable]
        public class SettingsData
        {
            public Environment[] Environments = new Environment[] { 
                new Environment { 
                    Name = "Default", 
                    Url = DefaultDevelopmentUrl, 
                    Type = EnvironmentType.Default
                } 
            };
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Bind<BackendEnvironment>().ToMethod<BackendEnvironment>(CreateEnvironments);
        }

        BackendEnvironment CreateEnvironments()
        {
            return new BackendEnvironment(Settings.Environments);
        }
    }
}