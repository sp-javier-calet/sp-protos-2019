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
        Container.Rebind<UnityUpdateRunner>().ToSingle<GameObject>();
        Container.Rebind<ICoroutineRunner>().ToLookup<UnityUpdateRunner>();
        Container.Rebind<IUpdateScheduler>().ToLookup<UnityUpdateRunner>();

        Container.Rebind<BreadcrumbManager>().ToSingle();
    }

    public void Initialize()
    {
        var scheduler = Container.Resolve<IUpdateScheduler>();
        var updateables = Container.OptResolve<List<IUpdateable>>();
        if(updateables != null)
        {
            scheduler.Add(updateables);
        }
    }
}
