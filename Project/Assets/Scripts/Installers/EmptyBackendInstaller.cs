using UnityEngine;
using Zenject;
using System;
using SocialPoint.Events;
using SocialPoint.Login;
using SocialPoint.ServerSync;

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
            Container.Bind<ILogin>().ToSingle<EmptyLogin>();
            Container.Bind<IDisposable>().ToSingle<EmptyLogin>();
        }
        if(!Container.HasBinding<ICommandQueue>())
        {
            Container.Bind<ICommandQueue>().ToSingle<EmptyCommandQueue>();
            Container.Bind<IDisposable>().ToSingle<EmptyCommandQueue>();
        }
    }
}
