using System;
using SocialPoint.Social;
using SocialPoint.Dependency;

public class SocialFrameworkInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
    }

    public override void InstallBindings()
    {       
        Container.Bind<ConnectionManager>().ToMethod<ConnectionManager>(CreateConnectionManager);
    }

    ConnectionManager CreateConnectionManager()
    {
        return new ConnectionManager();
    }
}
