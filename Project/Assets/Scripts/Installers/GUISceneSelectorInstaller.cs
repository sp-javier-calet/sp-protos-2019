//-----------------------------------------------------------------------
// GUISceneSelectorInstaller.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.Base;
using UnityEngine.SceneManagement;

[InstallerGameCategory]
public class GUISceneSelectorInstaller : Installer, IInitializable
{
    [Serializable]
    public class SettingsData
    {
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

        _scenes = ScenesData.Instance.ScenesNames;

        var entryScene = Settings.EntryScene;
        if(!string.IsNullOrEmpty(entryScene) && _scenes.Contains(entryScene))
        {
            GoToScene(entryScene);
            return;
        }

        var go = Instantiate(Settings.InitialScreenPrefab);
        var ctrl = go.GetComponent<SelectorScenesController>();
        if(ctrl == null)
        {
            throw new InvalidOperationException("Initial Screen Prefab does not contain a SelectorScenesController");
        }

        ctrl.OnGoToScene = GoToScene;
    }

    void GoToScene(string nameScene)
    {
        SceneManager.LoadScene(nameScene, LoadSceneMode.Single);
    }
}
