using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.Base;
using SocialPoint.Login;
using UnityEngine.SceneManagement;

public class GUISceneSelectorInstaller : Installer, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        public GameObject InitialScreenPrefab;
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

        var config = Container.Resolve<ConfigLoginEnvironment>();

        if(config.EntryScene != string.Empty && _scenes.Contains<string>(config.EntryScene))
        {
            GoToScene(config.EntryScene);
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