using System;
using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.Crash;
using SocialPoint.Utils;
using SocialPoint.Base;

public class BaseInstaller : MonoInstaller, IInitializable
{
    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToSingleInstance(this);
        Container.BindUnityComponent<UnityUpdateRunner>();
        Container.Rebind<ICoroutineRunner>().ToLookup<UnityUpdateRunner>();
        Container.Rebind<IUpdateScheduler>().ToLookup<UnityUpdateRunner>();
    }

    public void Initialize()
    {
        var scheduler = Container.Resolve<IUpdateScheduler>();
        var updateables = Container.ResolveArray<IUpdateable>();
        if(updateables != null)
        {
            scheduler.Add(updateables);
        }
    }
}
