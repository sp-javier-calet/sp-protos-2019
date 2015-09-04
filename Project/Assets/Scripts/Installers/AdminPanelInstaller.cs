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
        Container.Bind<AdminPanel>().ToSingle<AdminPanel>();

        Container.Bind<AdminPanelConfigurer>().ToSingle<AdminPanelLogin>();
        Container.Bind<AdminPanelConfigurer>().ToSingle<AdminPanelCrashReporter>();
        Container.Bind<AdminPanelConfigurer>().ToSingle<AdminPanelProfilerGUI>();
        Container.Bind<AdminPanelConfigurer>().ToSingle<AdminPanelHardware>();
        Container.Bind<AdminPanelConfigurer>().ToSingle<AdminPanelLogGUI>();
        Container.Bind<AdminPanelConfigurer>().ToSingle<AdminPanelAppEvents>();
        Container.Bind<AdminPanelConfigurer>().ToSingle<AdminPanelApplicationGUI>();
    }
}
