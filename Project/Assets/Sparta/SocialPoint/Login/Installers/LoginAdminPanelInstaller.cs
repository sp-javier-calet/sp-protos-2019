using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;

public class LoginAdminPanelInstaller : SubInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelLogin>(CreateAdminPanel);
    }

    AdminPanelLogin CreateAdminPanel()
    {
        var login = Container.Resolve<ILogin>();
        var appEvents = Container.Resolve<IAppEvents>();
        var environments = Container.Resolve<BackendEnvironment>();
        var envs = new Dictionary<string,string>();

        for(var i = 0; i < environments.Environments.Length; ++i)
        {
            var env = environments.Environments[i];
            envs.Add(env.Name, env.Url);
        }

        return new AdminPanelLogin(login, envs, appEvents);
    }
}
