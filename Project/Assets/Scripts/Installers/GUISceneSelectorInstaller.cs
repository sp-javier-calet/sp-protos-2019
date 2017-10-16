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
        public bool UsePrototypeConfig = false;
        public string EntryScene;
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

        string entryScene = string.Empty;

        var config = Container.Resolve<ConfigLoginEnvironment>();
        if(config != null && Settings.UsePrototypeConfig)
        {
            entryScene = config.EntryScene;
        }
        else
        {
            entryScene = Settings.EntryScene;
        }

        if(entryScene != string.Empty && _scenes.Contains<string>(entryScene))
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