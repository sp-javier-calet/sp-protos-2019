using System;
using SocialPoint.AppEvents;
using SocialPoint.CrossPromotion;
using SocialPoint.Dependency;
using SocialPoint.Locale;
using SocialPoint.Notifications;
using SocialPoint.Purchase;
using SocialPoint.ServerSync;
using SocialPoint.Social;
using SocialPoint.Utils;
using UnityEngine;

public class GameServicesInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        
        public CrossPromotionSettings CrossPromotion;
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

        // Purchase store
        Container.Bind<IStoreProductSource>().ToGetter<ConfigModel>((Config) => Config.Store);

        // Social Framework - Game chat rooms
        Container.Bind<IChatRoom>().ToMethod<ChatRoom<PublicChatMessage>>(CreatePublicChatRoom, SetupPublicChatRoom);
        Container.Bind<IChatRoom>().ToMethod<ChatRoom<AllianceChatMessage>>(CreateAllianceChatRoom, SetupAllianceChatRoom);
    }

    GameCrossPromotionManager CreateManager()
    {
        return new GameCrossPromotionManager(
            Container.Resolve<ICoroutineRunner>(),
            Container.Resolve<INativeUtils>(),
            Container.Resolve<PopupsController>());
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
