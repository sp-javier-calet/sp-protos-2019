using System;
using SocialPoint.Dependency;

namespace SocialPoint.Login
{
    public class PrototypeInstaller : Installer {

        [Serializable]
        public class SettingsData
        {
            public string EntryScene;
            public string GameId;
            public string Environment;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Bind<ConfigLoginEnvironment>().ToMethod<ConfigLoginEnvironment>(CreatePrototype);
        }

        ConfigLoginEnvironment CreatePrototype()
        {
            var config = new ConfigLoginEnvironment();
            config.EntryScene = Settings.EntryScene;
            config.GameId = Settings.GameId;
            config.Environment = Settings.Environment;

            return config;
        }
    }
}