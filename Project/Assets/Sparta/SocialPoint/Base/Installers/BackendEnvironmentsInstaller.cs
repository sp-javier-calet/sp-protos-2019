using System;
using SocialPoint.Dependency;

namespace SocialPoint.Base
{

    public class BackendEnvironmentsInstaller : Installer
    {
        [Serializable]
        public class SettingsData
        {
            public Environment[] Environments;
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