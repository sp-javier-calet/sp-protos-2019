using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.Base;
using UnityEngine.SceneManagement;

public class GUISceneSelectorInstaller : Installer, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        public string ForcedSceneName = string.Empty;
        public GameObject InitialScreenPrefab;
        public bool InitialScreenAnimation;
    }

    public SettingsData Settings = new SettingsData();

    private string[] _scenes;

    public GUISceneSelectorInstaller() : base(ModuleType.Game)
    {
    }

    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToInstance(this);
    }

    public void Initialize()
    {
        if(Settings.InitialScreenPrefab == null)
        {
            return;
        }

        var stackController = Container.Resolve<UIStackController>();
        if(stackController == null)
        {
            throw new InvalidOperationException("Could not find screens controller for initial screen");
        }

        _scenes = ScenesData.Instance.ScenesNames;
        if(Settings.ForcedSceneName != string.Empty && _scenes.Contains<string>(Settings.ForcedSceneName))
        {
            GoToScene(Settings.ForcedSceneName);
            return;
        }

        var go = Instantiate<GameObject>(Settings.InitialScreenPrefab);
        var ctrl = go.GetComponent<SelectorScenesController>();
        if(ctrl == null)
        {
            throw new InvalidOperationException("Initial Screen Prefab does not contain a SelectorScenesController");
        }

        ctrl.OnGoToScene = GoToScene;

        if(Settings.InitialScreenAnimation)
        {
            stackController.Push(ctrl);
        }
        else
        {
            stackController.PushImmediate(ctrl);
        }
    }

    void GoToScene(string nameScene)
    {
        SceneManager.LoadScene(nameScene, LoadSceneMode.Additive);
    }
}