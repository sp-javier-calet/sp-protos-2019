using System;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Social;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;

public class LoginAdminPanelInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelLogin>(CreateAdminPanel);
    }

    AdminPanelLogin CreateAdminPanel()
    {
        var login = Container.Resolve<ILogin>();
        var appEvents = Container.Resolve<IAppEvents>();
        var envs = new Dictionary<string,string>();
        foreach(BackendEnvironment env in Enum.GetValues(typeof(BackendEnvironment)))
        {
            envs.Add(env.ToString(), env.GetUrl());
        }

        return new AdminPanelLogin(login, envs, appEvents);
    }
}
