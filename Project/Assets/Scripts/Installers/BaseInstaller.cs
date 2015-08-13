using System;
using Zenject;
using UnityEngine;

public class BaseInstaller : MonoInstaller
{   
    public override void InstallBindings()
    {
        Container.Bind<MonoBehaviour>().ToSingleGameObject();
        Container.BindAllInterfacesToSingle<GameParser>();
    }
}