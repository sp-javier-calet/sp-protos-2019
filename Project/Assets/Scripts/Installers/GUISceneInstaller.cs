//-----------------------------------------------------------------------
// GUISceneInstaller.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;

public class GUISceneInstaller : Installer, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        public GameObject InitialScreenPrefab;
        public bool InitialScreenAnimation;
    }

    public SettingsData Settings = new SettingsData();

    public GUISceneInstaller() : base(ModuleType.Game)
    {
    }

    public override void InstallBindings(IBindingContainer container)
    {
        container.Bind<IInitializable>().ToInstance(this);
    }

    public void Initialize(IResolutionContainer container)
    {
        if(Settings.InitialScreenPrefab == null)
        {
            return;
        }

        var stackController = container.Resolve<UIStackController>();
        if(stackController == null)
        {
            throw new InvalidOperationException("Could not find screens controller for initial screen");
        }

        var go = Instantiate<GameObject>(Settings.InitialScreenPrefab);
        var ctrl = go.GetComponent<UIViewController>();
        if(ctrl == null)
        {
            throw new InvalidOperationException("Initial Screen Prefab does not contain a UIViewController");
        }

        if(Settings.InitialScreenAnimation)
        {
            stackController.Push(ctrl);
        }
        else
        {
            stackController.PushImmediate(ctrl);
        }
    }
}