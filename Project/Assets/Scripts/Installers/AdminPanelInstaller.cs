using Zenject;
using System;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Profiler;

public class AdminPanelInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        if(Container.HasBinding<AdminPanel>())
        {
            return;
        }
        Container.Bind<AdminPanel>().ToSingle<AdminPanel>();       
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelGame>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelProfilerGUI>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLogGUI>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelApplicationGUI>();
    }
}
