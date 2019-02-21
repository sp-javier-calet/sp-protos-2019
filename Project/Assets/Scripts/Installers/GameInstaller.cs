using System;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.GameLoading;
using SocialPoint.Helpshift;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Restart;
using SocialPoint.Social;
using SocialPoint.Tutorial;
using SocialPoint.Utils;
#if ADMIN_PANEL
using SocialPoint.AdminPanel;

#endif

public class GameInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public bool EditorDebug = true;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings(IBindingContainer container)
    {
#if UNITY_EDITOR
        container.Bind<bool>("game_debug").ToInstance(Settings.EditorDebug);
#else
        container.Bind<bool>("game_debug").ToInstance(UnityEngine.Debug.isDebugBuild);
#endif
        container.Bind<IGameErrorHandler>().ToMethod(CreateErrorHandler);
        container.Bind<IDisposable>().ToLookup<IGameErrorHandler>();

#if ADMIN_PANEL
        container.BindAdminPanelConfigurer(CreateAdminPanel);
#endif

        container.Bind<IPlayerData>().ToMethod(CreatePlayerData);
        container.Bind<TutorialManager>().ToMethod(CreateTutorialManager);
    }

#if ADMIN_PANEL
    AdminPanelGame CreateAdminPanel(IResolutionContainer container)
    {
        return new AdminPanelGame(
            container.Resolve<IAppEvents>());
    }
#endif

    GameErrorHandler CreateErrorHandler(IResolutionContainer container)
    {
        return new GameErrorHandler(
            container.Resolve<IAlertView>(),
            container.Resolve<Localization>(),
            container.Resolve<IAppEvents>(),
            container.Resolve<IRestarter>(),
            container.Resolve<IHelpshift>(),
            container.Resolve<bool>("game_debug"));
    }

    PlayerDataProvider CreatePlayerData(IResolutionContainer container)
    {
        return new PlayerDataProvider(
            container.Resolve<ILoginData>());
    }

    TutorialManager CreateTutorialManager(IResolutionContainer container)
    {
        return new TutorialManager(container.Resolve<IUpdateScheduler>());
    }

    /// <summary>
    /// Game must integrate data to implement an unique provider of player data
    /// </summary>
    class PlayerDataProvider : IPlayerData
    {
        readonly ILoginData _loginData;

        public PlayerDataProvider(ILoginData loginData)
        {
            _loginData = loginData;
        }

#region IPlayerData implementation

        public string Id => _loginData.UserId.ToString();

        public string Name => "Player Name";

        public int Level => 0; // TODO IVAN

#endregion
    }
}
