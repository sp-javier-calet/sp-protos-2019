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
        Container.Bind<GameCrossPromotionManager>().ToMethod<GameCrossPromotionManager>(CreateManager);
        Container.Listen<GameCrossPromotionManager>().Then(SetupManager);
        Container.Bind<CrossPromotionManager>().ToLookup<GameCrossPromotionManager>();
        Container.Bind<IDisposable>().ToLookup<CrossPromotionManager>();

        // Notifications
        Container.Bind<GameNotificationManager>().ToMethod<GameNotificationManager>(CreateNotificationManager);
        Container.Bind<NotificationManager>().ToLookup<GameNotificationManager>();
        Container.Bind<IDisposable>().ToLookup<GameNotificationManager>();

        // Purchase store
        Container.Bind<IStoreProductSource>().ToGetter<ConfigModel>((Config) => Config.Store);

        // Social Framework - Game chat rooms
        Container.Bind<ChatRoom<PublicChatMessage>>().ToMethod<ChatRoom<PublicChatMessage>>(CreatePublicChatRoom);
        Container.Bind<IChatRoom>().ToLookup<ChatRoom<PublicChatMessage>>();
        Container.Listen<ChatRoom<PublicChatMessage>>().Then(SetupPublicChatRoom);
        Container.Bind<ChatRoom<AllianceChatMessage>>().ToMethod<ChatRoom<AllianceChatMessage>>(CreateAllianceChatRoom);
        Container.Bind<IChatRoom>().ToLookup<ChatRoom<AllianceChatMessage>>();
        Container.Listen<ChatRoom<AllianceChatMessage>>().Then(SetupAllianceChatRoom);
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
