using System;
using SocialPoint.AppEvents;
using SocialPoint.CrossPromotion;
using SocialPoint.Dependency;
using SocialPoint.ServerSync;
using SocialPoint.ServerEvents;
using SocialPoint.ScriptEvents;
using SocialPoint.Locale;
using SocialPoint.Notifications;
using SocialPoint.Purchase;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.Utils;

public class GameServicesInstaller : Installer
{
    public enum EnvironmentID
    {
        dev,
        loc,
        prod
    }

    [Serializable]
    public class SettingsData
    {
        public EnvironmentID EnvironmentId = EnvironmentID.prod;
        public string ProjectId = GameLocalizationManager.LocationData.DefaultProjectId;
        public string SecretKeyDev = GameLocalizationManager.LocationData.DefaultSecretKey;
        public string SecretKeyLoc = GameLocalizationManager.LocationData.DefaultSecretKey;
        public string SecretKeyProd = GameLocalizationManager.LocationData.DefaultSecretKey;
        public string BundleDir = GameLocalizationManager.DefaultBundleDir;
        public string[] SupportedLanguages = GameLocalizationManager.DefaultSupportedLanguages;
        public float Timeout = GameLocalizationManager.DefaultTimeout;
    }

    public SettingsData Settings = new SettingsData();


    public override void InstallBindings()
    {
        // CrossPromotion
        Container.Bind<GameCrossPromotionManager>().ToMethod<GameCrossPromotionManager>(CreateManager, SetupManager);
        Container.Bind<CrossPromotionManager>().ToLookup<GameCrossPromotionManager>();
        Container.Bind<IDisposable>().ToLookup<CrossPromotionManager>();

        // Notifications
        Container.Rebind<GameNotificationManager>().ToMethod<GameNotificationManager>(CreateNotificationManager);
        Container.Rebind<NotificationManager>().ToLookup<GameNotificationManager>();
        Container.Bind<IDisposable>().ToMethod<GameNotificationManager>(CreateNotificationManager);

        // Localization
        Container.Rebind<ILocalizationManager>().ToMethod<GameLocalizationManager>(CreateLocalizationManager, SetupLocalizationManager);
        Container.Bind<IDisposable>().ToLookup<ILocalizationManager>();

        // Purchase store
        Container.Bind<IStoreProductSource>().ToGetter<ConfigModel>((Config) => Config.Store);
    }

    GameCrossPromotionManager CreateManager()
    {
        return new GameCrossPromotionManager(Container.Resolve<ICoroutineRunner>(), Container.Resolve<PopupsController>());
    }

    void SetupManager(CrossPromotionManager mng)
    {
        // TODO how to move to sparta?
        var eventTracker = Container.Resolve<IEventTracker>();
        mng.TrackSystemEvent = eventTracker.TrackSystemEvent;
        mng.TrackUrgentSystemEvent = eventTracker.TrackUrgentSystemEvent;
        mng.AppEvents = Container.Resolve<IAppEvents>();
    }

    GameNotificationManager CreateNotificationManager()
    {
        return new GameNotificationManager(
            Container.Resolve<INotificationServices>(),
            Container.Resolve<IAppEvents>(),
            Container.Resolve<ICommandQueue>()
        );
    }

    GameLocalizationManager CreateLocalizationManager()
    {
        return new GameLocalizationManager(
            Container.Resolve<IHttpClient>(),
            Container.Resolve<IAppInfo>(),
            Container.Resolve<Localization>(),
            Container.Resolve<LocalizeAttributeConfiguration>(),
            Container.Resolve<IEventDispatcher>());
    }

    void SetupLocalizationManager(GameLocalizationManager mng)
    {
        string secretKey;
        if(Settings.EnvironmentId == EnvironmentID.dev)
        {
            secretKey = Settings.SecretKeyDev;
        }
        else if(Settings.EnvironmentId == EnvironmentID.loc)
        {
            secretKey = Settings.SecretKeyLoc;
        }
        else
        {
            secretKey = Settings.SecretKeyProd;
        }
        mng.Location.ProjectId = Settings.ProjectId;
        mng.Location.EnvironmentId = Settings.EnvironmentId.ToString();
        mng.Location.SecretKey = secretKey;
        mng.Timeout = Settings.Timeout;
        mng.BundleDir = Settings.BundleDir;
        mng.AppEvents = Container.Resolve<IAppEvents>();
    }
}
