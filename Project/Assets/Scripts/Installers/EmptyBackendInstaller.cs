using System;
using Zenject;
using SocialPoint.Attributes;
using SocialPoint.Events;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Crash;
using System.Text;

public class EmptyBackendInstaller : MonoInstaller
{
    [Inject]
    GameLoader _gameLoader;
    
    [Inject]
    IParser<GameModel> _gameParser;
    
    public override void InstallBindings()
    {
        if(!Container.HasBinding<IEventTracker>())
        {
            Container.Bind<IEventTracker>().ToSingle<EmptyEventTracker>();
            Container.Bind<IDisposable>().ToLookup<IEventTracker>();
        }
        if(!Container.HasBinding<ILogin>())
        {
            Container.Bind<ILogin>().ToSingleMethod<EmptyLogin>(CreateEmptyLogin);
            Container.Bind<IDisposable>().ToLookup<ILogin>();
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
        }
        _gameLoader.LoadInitial();
    }

    EmptyLogin CreateEmptyLogin(InjectContext ctx)
    {
        return new EmptyLogin();
    }

}