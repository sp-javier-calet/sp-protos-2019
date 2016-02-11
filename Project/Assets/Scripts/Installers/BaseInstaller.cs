using System;
using Zenject;
using UnityEngine;
using SocialPoint.Crash;
using SocialPoint.Utils;
using SocialPoint.Base;

public class BaseInstaller : MonoInstaller, IInitializable
{
    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToSingleInstance(this);
        Container.Rebind<UnityUpdateRunner>().ToSingleGameObject<UnityUpdateRunner>();
        Container.Rebind<ICoroutineRunner>().ToLookup<UnityUpdateRunner>();
        Container.Rebind<IUpdateScheduler>().ToLookup<UnityUpdateRunner>();
        Container.Rebind<IUnityDownloader>().ToLookup<UnityUpdateRunner>();

        Container.Rebind<BreadcrumbManager>().ToSingle();
    }

    public void Initialize()
    {
        Container.Resolve<IUpdateScheduler>();
    }
}
