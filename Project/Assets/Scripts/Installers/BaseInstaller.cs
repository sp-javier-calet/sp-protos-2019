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
        Container.Bind<ICoroutineRunner>().ToSingleGameObject<UnityUpdateRunner>();
        Container.Bind<IUpdateScheduler>().ToSingleGameObject<UnityUpdateRunner>();
        Container.Bind<IUnityDownloader>().ToSingleGameObject<UnityUpdateRunner>();
        Container.Rebind<BreadcrumbManager>().ToSingle();
    }

    public void Initialize()
    {
        Container.Resolve<IUpdateScheduler>();
    }
}
