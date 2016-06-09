using System;
using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.Crash;
using SocialPoint.Utils;
using SocialPoint.Base;
using SocialPoint.AdminPanel;

public class BaseInstaller : Installer, IInitializable
{
    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToInstance(this);
        Container.BindUnityComponent<UnityUpdateRunner>();
        Container.Rebind<ICoroutineRunner>().ToLookup<UnityUpdateRunner>();
        Container.Rebind<IUpdateScheduler>().ToLookup<UnityUpdateRunner>();
        Container.Bind<NativeCallsHandler>().ToMethod<NativeCallsHandler>(CreateNativeCallsHandler);
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelNativeCallsHandler>(CreateAdminPanel);
    }

    public void Initialize()
    {
        var scheduler = Container.Resolve<IUpdateScheduler>();
        var updateables = Container.ResolveList<IUpdateable>().ToArray();
        if(updateables != null)
        {
            scheduler.Add(updateables);
        }
    }

    NativeCallsHandler CreateNativeCallsHandler()
    {
        var trans = Container.Resolve<Transform>();
        var go = new GameObject();
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.transform.SetParent(trans);
        return go.AddComponent<NativeCallsHandler>();
    }

    AdminPanelNativeCallsHandler CreateAdminPanel()
    {
        return new AdminPanelNativeCallsHandler(
            Container.Resolve<NativeCallsHandler>());
    }
}
