using System;
using Zenject;
using SocialPoint.Attributes;
using SocialPoint.ServerEvents;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Crash;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using System.Text;

public class EmptyBackendInstaller : MonoInstaller
{
    [Inject]
    IGameLoader _gameLoader;

    public override void InstallBindings()
    {
        if(!Container.HasBinding<IEventTracker>())
        {
            Container.Bind<IEventTracker>().ToSingle<EmptyEventTracker>();
            Container.Bind<IDisposable>().ToLookup<IEventTracker>();
        }
        if(!Container.HasBinding<ILogin>())
        {
            Container.Bind<ILogin>().ToSingle<EmptyLogin>();
            Container.Bind<IDisposable>().ToLookup<ILogin>();
            Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelLogin>(LoginInstaller.CreateAdminPanel);
        }
        if(!Container.HasBinding<ICommandQueue>())
        {
            Container.Bind<ICommandQueue>().ToSingle<EmptyCommandQueue>();
            Container.Bind<IDisposable>().ToLookup<ICommandQueue>();
        }
        if(!Container.HasBinding<ICrashReporter>())
        {
            Container.Bind<ICrashReporter>().ToSingle<EmptyCrashReporter>();
            Container.Bind<IDisposable>().ToLookup<ICrashReporter>();
            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelCrashReporter>();
        }
        if(_gameLoader != null)
        {
            _gameLoader.Load(null);
        }
    }

}
