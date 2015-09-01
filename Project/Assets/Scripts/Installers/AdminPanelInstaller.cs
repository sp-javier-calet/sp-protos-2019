using Zenject;
using System;
using SocialPoint.AdminPanel;

public class AdminPanelInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<AdminPanel>().ToSingle<AdminPanel>();
    }
}
