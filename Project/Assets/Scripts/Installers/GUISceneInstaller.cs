//-----------------------------------------------------------------------
// GUISceneInstaller.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using UnityEngine;
using SocialPoint.Dependency;

[InstallerGameCategory]
public class GUISceneInstaller : Installer, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        public GameObject InitialScreenPrefab;
    }

    public SettingsData Settings = new SettingsData();

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

        Instantiate(Settings.InitialScreenPrefab);
    }
}
