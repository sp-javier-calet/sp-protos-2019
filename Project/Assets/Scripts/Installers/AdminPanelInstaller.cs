using Zenject;
using System;
using SocialPoint.AdminPanel;
using SocialPoint.Profiler;
using SocialPoint.Utils;
using SocialPoint.AppEvents;

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
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLogin>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelCrashReporter>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelProfilerGUI>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelHardware>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLogGUI>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelAppEvents>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelApplicationGUI>();
    }
}
