using System;
using Zenject;
using UnityEngine;
using SocialPoint.Crash;
using SocialPoint.Utils;
using SocialPoint.Profiling;

public class BaseInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Rebind<MonoBehaviour>().ToSingleGameObject();
        Container.Rebind<BreadcrumbManager>().ToSingle();
        Container.Rebind<SceneManager>().ToSingleInstance(SceneManager.Instance);
    }
}
