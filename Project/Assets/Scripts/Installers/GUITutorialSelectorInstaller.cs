using System;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.Tutorial;
using UnityEngine;

public class GUITutorialSelectorInstaller : Installer, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        public GameObject InitialScreenPrefab;
        public TutorialDataList Tutorials;
    }
    
    public SettingsData Settings = new SettingsData();

    public GUITutorialSelectorInstaller() : base(ModuleType.Game)
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
        
        var tutorialManager = container.Resolve<TutorialManager>();
        if(tutorialManager == null)
        {
            throw new InvalidOperationException("Could not find tutorial manager for tutorials selector");
        }
        
        var tutorialsList = Settings.Tutorials;
        if(tutorialsList == null || tutorialsList.Tutorials == null)
        {
            throw new InvalidOperationException("Invalid tutorial list");
        }
        
        foreach(var tutorial in tutorialsList.Tutorials)
        {
            foreach(var step in tutorial.Steps)
            {
                if(step.Object == null)
                {
                    throw new InvalidOperationException("Could not find tutorial step object");
                }
            }            
            tutorialManager.Add(tutorial); 
        }

        var go = Instantiate<GameObject>(Settings.InitialScreenPrefab);
        var ctrl = go.GetComponent<SelectorTutorialsController>();
        if(ctrl == null)
        {
            throw new InvalidOperationException("Initial Screen Prefab does not contain a SelectorTutorialsController");
        }

        stackController.PushImmediate(ctrl);
    }
}
