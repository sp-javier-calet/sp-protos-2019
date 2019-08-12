﻿//-----------------------------------------------------------------------
// GUISceneSelectorInstaller.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.Base;
using SocialPoint.Login;
using UnityEngine.SceneManagement;

[InstallerGameCategory]
public class GUISceneSelectorInstaller : Installer, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        public bool UsePrototypeConfig = false;
        public string EntryScene;
        public GameObject InitialScreenPrefab;
    }

    public SettingsData Settings = new SettingsData();

    private string[] _scenes;

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

        _scenes = ScenesData.Instance.ScenesNames;


        var entryScene = Settings.EntryScene;
        if(!string.IsNullOrEmpty(entryScene) && _scenes.Contains(entryScene))
        {
            GoToScene(entryScene);
            return;
        }

        var go = Instantiate<GameObject>(Settings.InitialScreenPrefab);
        var ctrl = go.GetComponent<SelectorScenesController>();
        if(ctrl == null)
        {
            throw new InvalidOperationException("Initial Screen Prefab does not contain a SelectorScenesController");
        }

        ctrl.OnGoToScene = GoToScene;

        stackController.PushImmediate(ctrl);
    }

    void GoToScene(string nameScene)
    {
        SceneManager.LoadScene(nameScene, LoadSceneMode.Single);
    }
}
