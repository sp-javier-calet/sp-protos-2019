using UnityEngine;
using Zenject;
using System;
using SocialPoint.Events;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Social;

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
            Container.Bind<ILogin>().ToSingleMethod<EmptyLogin>(CreateEmptyLogin);
            Container.Bind<IDisposable>().ToSingleMethod<EmptyLogin>(CreateEmptyLogin);
        }
        if(!Container.HasBinding<ICommandQueue>())
        {
            Container.Bind<ICommandQueue>().ToSingle<EmptyCommandQueue>();
            Container.Bind<IDisposable>().ToSingle<EmptyCommandQueue>();
        }
    }

    EmptyLogin CreateEmptyLogin(InjectContext ctx)
    {
        return new EmptyLogin();
    }
}
