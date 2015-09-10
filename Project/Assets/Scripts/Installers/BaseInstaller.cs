using System;
using Zenject;
using UnityEngine;

public class BaseInstaller : MonoInstaller
{   
    public override void InstallBindings()
    {
        if(!Container.HasBinding<MonoBehaviour>())
        {
            Container.Bind<MonoBehaviour>().ToSingleGameObject();
        }
        if(!Container.HasBinding<GameParser>())
        {
            Container.BindAllInterfacesToSingle<GameParser>();
        }
    }
}