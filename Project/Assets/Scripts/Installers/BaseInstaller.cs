using System;
using Zenject;
using UnityEngine;
using SocialPoint.Crash;
using SocialPoint.Utils;

public class BaseInstaller : MonoInstaller
{   
    public override void InstallBindings()
    {
        if(!Container.HasBinding<MonoBehaviour>())
        {
            Container.Bind<MonoBehaviour>().ToSingleGameObject();
        }
        if(!Container.HasBinding<BreadcrumbManager>())
        {
            Container.Bind<BreadcrumbManager>().ToSingle();
        }
        if(!Container.HasBinding<SceneManager>())
        {
            Container.Bind<SceneManager>().ToInstance(SceneManager.Instance);
        }
    }
}