﻿//-----------------------------------------------------------------------
// GUITutorialSelectorInstaller.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using SocialPoint.Dependency;
using SocialPoint.Tutorial;
using UnityEngine;

[InstallerGameCategory]
public class GUITutorialSelectorInstaller : Installer, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        public GameObject InitialScreenPrefab;
        public TutorialDataList Tutorials;
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

        var tutorialManager = container.Resolve<ITutorialManager>();
        if(tutorialManager == null)
        {
            throw new InvalidOperationException("Could not find tutorial manager for tutorials selector");
        }

        var tutorialsList = Settings.Tutorials;
        if(tutorialsList == null || tutorialsList.Tutorials == null)
        {
            throw new InvalidOperationException("Invalid tutorial list");
        }

        tutorialManager.AddTutorials(tutorialsList.Tutorials);

        var go = Instantiate(Settings.InitialScreenPrefab);
        var ctrl = go.GetComponent<SelectorTutorialsController>();
        if(ctrl == null)
        {
            throw new InvalidOperationException("Initial Screen Prefab does not contain a SelectorTutorialsController");
        }
    }
}
