using System;
using UnityEngine;
using SocialPoint.AppEvents;
using SocialPoint.CrossPromotion;
using SocialPoint.Dependency;
using SocialPoint.ServerSync;
using SocialPoint.ScriptEvents;
using SocialPoint.Locale;
using SocialPoint.Notifications;
using SocialPoint.Purchase;
using SocialPoint.Network;
using SocialPoint.Social;
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
        public LocalizationSettings Localization;
        public CrossPromotionSettings CrossPromotion;
    }

    [Serializable]
    public class LocalizationSettings
    {
        public EnvironmentID EnvironmentId = EnvironmentID.prod;
        public string ProjectId = GameLocalizationManager.LocationData.DefaultProjectId;
        public string SecretKeyDev = GameLocalizationManager.LocationData.DefaultDevSecretKey;
        public string SecretKeyLoc = GameLocalizationManager.LocationData.DefaultDevSecretKey;
        public string SecretKeyProd = GameLocalizationManager.LocationData.DefaultProdSecretKey;
        public string BundleDir = GameLocalizationManager.DefaultBundleDir;
        public string[] SupportedLanguages = GameLocalizationManager.DefaultSupportedLanguages;
        public float Timeout = GameLocalizationManager.DefaultTimeout;
    }

    [Serializable]
    public class CrossPromotionSettings
    {
        public GameObject ButtonPrefab;
        public GameObject PopupPrefab;
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
        Container.Bind<IDisposable>().ToLookup<GameNotificationManager>();

        // Localization
        Container.Rebind<ILocalizationManager>().ToMethod<GameLocalizationManager>(CreateLocalizationManager, SetupLocalizationManager);
        Container.Bind<IDisposable>().ToLookup<ILocalizationManager>();

        // Purchase store
        Container.Bind<IStoreProductSource>().ToGetter<ConfigModel>((Config) => Config.Store);

        // Social Framework - Game chat rooms
        Container.Bind<IChatRoom>().ToMethod<ChatRoom<PublicChatMessage>>(CreatePublicChatRoom, SetupPublicChatRoom);
        Container.Bind<IChatRoom>().ToMethod<ChatRoom<AllianceChatMessage>>(CreateAllianceChatRoom, SetupAllianceChatRoom);
    }

    GameCrossPromotionManager CreateManager()
    {
        return new GameCrossPromotionManager(Container.Resolve<ICoroutineRunner>(), Container.Resolve<PopupsController>());
    }

    void SetupManager(GameCrossPromotionManager manager)
    {
        manager.ButtonPrefab = Settings.CrossPromotion.ButtonPrefab;
        manager.PopupPrefab = Settings.CrossPromotion.PopupPrefab;
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
            Container.Resolve<LocalizeAttributeConfiguration>(),
            Container.Resolve<IEventDispatcher>());
    }

    void SetupLocalizationManager(GameLocalizationManager mng)
    {
        mng.HttpClient = Container.Resolve<IHttpClient>();
        mng.AppInfo = Container.Resolve<IAppInfo>();
        mng.Localization = Container.Resolve<Localization>();

        string secretKey;
        if(Settings.Localization.EnvironmentId == EnvironmentID.dev)
        {
            secretKey = Settings.Localization.SecretKeyDev;
        }
        else if(Settings.Localization.EnvironmentId == EnvironmentID.loc)
        {
            secretKey = Settings.Localization.SecretKeyLoc;
        }
        else
        {
            secretKey = Settings.Localization.SecretKeyProd;
        }
        mng.Location.ProjectId = Settings.Localization.ProjectId;
        mng.Location.EnvironmentId = Settings.Localization.EnvironmentId.ToString();
        mng.Location.SecretKey = secretKey;
        mng.Timeout = Settings.Localization.Timeout;
        mng.BundleDir = Settings.Localization.BundleDir;

        mng.UpdateDefaultLanguage();
    }

    ChatRoom<PublicChatMessage> CreatePublicChatRoom()
    {
        return new ChatRoom<PublicChatMessage>("public");
    }

    void SetupPublicChatRoom(ChatRoom<PublicChatMessage> room)
    {
        room.ChatManager = Container.Resolve<ChatManager>();
        room.Localization = Container.Resolve<Localization>();

        // Configure optional events to manage custom data
        room.ParseUnknownNotifications = PublicChatMessage.ParseUnknownNotifications;
        room.ParseExtraInfo = PublicChatMessage.ParseExtraInfo;
        room.SerializeExtraInfo = PublicChatMessage.SerializeExtraInfo;
    }

    ChatRoom<AllianceChatMessage> CreateAllianceChatRoom()
    {
        return new ChatRoom<AllianceChatMessage>("alliance");
    }

    void SetupAllianceChatRoom(ChatRoom<AllianceChatMessage> room)
    {
        // Configure optional events to manage custom data
        room.ParseUnknownNotifications = AllianceChatMessage.ParseUnknownNotifications;
        room.ParseExtraInfo = AllianceChatMessage.ParseExtraInfo;
        room.SerializeExtraInfo = AllianceChatMessage.SerializeExtraInfo;
    }
}
