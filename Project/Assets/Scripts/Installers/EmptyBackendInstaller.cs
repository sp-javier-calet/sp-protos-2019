using UnityEngine;
using Zenject;
using System;
using SocialPoint.Events;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Crash;

public class EmptyBackendInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        if(!Container.HasBinding<IEventTracker>())
        {
            Container.Bind<IEventTracker>().ToSingle<EmptyEventTracker>();
            Container.Bind<IDisposable>().ToSingle<EmptyEventTracker>();
        }
        if(!Container.HasBinding<ILogin>())
        {
            Container.Bind<ILogin>().ToMethod(CreateEmptyLogin);
            Container.Bind<IDisposable>().ToMethod(CreateEmptyLogin);
        }
        if(!Container.HasBinding<ICommandQueue>())
        {
            Container.Bind<ICommandQueue>().ToSingle<EmptyCommandQueue>();
            Container.Bind<IDisposable>().ToSingle<EmptyCommandQueue>();
        }
        if(!Container.HasBinding<ICrashReporter>())
        {
            Container.Bind<ICrashReporter>().ToSingle<EmptyCrashReporter>();
        }
    }

    EmptyLogin _emptyLogin = null;
    EmptyLogin CreateEmptyLogin(InjectContext ctx)
    {
        if(_emptyLogin == null)
        {
            _emptyLogin = new EmptyLogin();
        }
        return _emptyLogin;
    }
}
