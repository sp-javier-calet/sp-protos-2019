
using System;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.GameLoading;
using SocialPoint.Alert;
using SocialPoint.Locale;
using SocialPoint.AppEvents;
using SocialPoint.ScriptEvents;
using SocialPoint.Login;

public class GameInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public string InitialJsonGameResource = "game";
        public string InitialJsonPlayerResource = "user";
        public bool EditorDebug = true;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
#if UNITY_EDITOR
        Container.BindInstance("game_debug", Settings.EditorDebug);
#else
        Container.BindInstance("game_debug", UnityEngine.Debug.isDebugBuild);
#endif
        Container.Install<GameModelInstaller>();

        Container.Rebind<IGameErrorHandler>().ToMethod<GameErrorHandler>(CreateErrorHandler);
        Container.Bind<IDisposable>().ToLookup<IGameErrorHandler>();

        Container.Rebind<IGameLoader>().ToMethod<GameLoader>(CreateGameLoader);
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelGame>(CreateAdminPanel);

        Container.Install<EconomyInstaller>();
    }

    AdminPanelGame CreateAdminPanel()
    {
        return new AdminPanelGame(
            Container.Resolve<IAppEvents>(),
            Container.Resolve<IGameLoader>(),
            Container.Resolve<GameModel>());
    }

    GameLoader CreateGameLoader()
    {
        return new GameLoader(
            Settings.InitialJsonGameResource,
            Settings.InitialJsonPlayerResource,
            Container.Resolve<IParser<GameModel>>(),
            Container.Resolve<IParser<ConfigModel>>(),
            Container.Resolve<IParser<PlayerModel>>(),
            Container.Resolve<ISerializer<PlayerModel>>(),
            Container.Resolve<GameModel>(),
            Container.Resolve<ILogin>());
    }

    GameErrorHandler CreateErrorHandler()
    {
        return new GameErrorHandler(
            Container.Resolve<IAlertView>(),
            Container.Resolve<Localization>(),
            Container.Resolve<IAppEvents>(),
            Container.Resolve<bool>("game_debug"));

    }
}
