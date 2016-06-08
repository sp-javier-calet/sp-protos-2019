using System;
using Zenject;
using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Crash;
using SocialPoint.Utils;
using SocialPoint.Base;

public class BaseInstaller : MonoInstaller, IInitializable
{
    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToSingleInstance(this);
        Container.Rebind<UnityUpdateRunner>().ToSingleGameObject();
        Container.Rebind<ICoroutineRunner>().ToLookup<UnityUpdateRunner>();
        Container.Rebind<IUpdateScheduler>().ToLookup<UnityUpdateRunner>();

        Container.Rebind<IBreadcrumbManager>().ToSingle<BreadcrumbManager>();
    }

    public void Initialize()
    {
        var scheduler = Container.Resolve<IUpdateScheduler>();
        var updateables = Container.TryResolve<List<IUpdateable>>();
        if(updateables != null)
        {
            scheduler.Add(updateables);
        }
    }
}
