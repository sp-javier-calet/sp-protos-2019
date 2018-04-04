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


    public override void InstallBindings(IBindingContainer container)
    {
        // CrossPromotion
        container.Bind<GameCrossPromotionManager>().ToMethod<GameCrossPromotionManager>(CreateManager);
        container.Listen<GameCrossPromotionManager>().Then(SetupManager);
        container.Bind<CrossPromotionManager>().ToLookup<GameCrossPromotionManager>();
        container.Bind<IDisposable>().ToLookup<CrossPromotionManager>();

        // Notifications
        container.Bind<GameNotificationManager>().ToMethod<GameNotificationManager>(CreateNotificationManager);
        container.Bind<NotificationManager>().ToLookup<GameNotificationManager>();
        container.Bind<IDisposable>().ToLookup<GameNotificationManager>();

        // Purchase store
        container.Bind<IStoreProductSource>().ToGetter<ConfigModel>((Config) => Config.Store);

        // Social Framework - Game chat rooms
        container.Bind<ChatRoom<PublicChatMessage>>().ToMethod<ChatRoom<PublicChatMessage>>(CreatePublicChatRoom);
        container.Bind<IChatRoom>().ToLookup<ChatRoom<PublicChatMessage>>();
        container.Listen<ChatRoom<PublicChatMessage>>().Then(SetupPublicChatRoom);
        container.Bind<ChatRoom<AllianceChatMessage>>().ToMethod<ChatRoom<AllianceChatMessage>>(CreateAllianceChatRoom);
        container.Bind<IChatRoom>().ToLookup<ChatRoom<AllianceChatMessage>>();
        container.Listen<ChatRoom<AllianceChatMessage>>().Then(SetupAllianceChatRoom);
    }

    GameCrossPromotionManager CreateManager(IResolutionContainer container)
    {
        return new GameCrossPromotionManager(
            container.Resolve<ICoroutineRunner>(),
            container.Resolve<INativeUtils>(),
            container.Resolve<PopupsController>());
    }

    void SetupManager(IResolutionContainer container, GameCrossPromotionManager manager)
    {
        manager.ButtonPrefab = Settings.CrossPromotion.ButtonPrefab;
        manager.PopupPrefab = Settings.CrossPromotion.PopupPrefab;
    }

    GameNotificationManager CreateNotificationManager(IResolutionContainer container)
    {
        return new GameNotificationManager(
            container.Resolve<INotificationServices>(),
            container.Resolve<IAppEvents>(),
            container.Resolve<ICommandQueue>()
        );
    }

    ChatRoom<PublicChatMessage> CreatePublicChatRoom(IResolutionContainer container)
    {
        return new ChatRoom<PublicChatMessage>("public");
    }

    void SetupPublicChatRoom(IResolutionContainer container, ChatRoom<PublicChatMessage> room)
    {
        room.ChatManager = container.Resolve<ChatManager>();
        room.Localization = container.Resolve<Localization>();

        // Configure optional events to manage custom data
        room.ParseUnknownNotifications = PublicChatMessage.ParseUnknownNotifications;
        room.ParseExtraInfo = PublicChatMessage.ParseExtraInfo;
        room.SerializeExtraInfo = PublicChatMessage.SerializeExtraInfo;
    }

    ChatRoom<AllianceChatMessage> CreateAllianceChatRoom(IResolutionContainer container)
    {
        return new ChatRoom<AllianceChatMessage>("alliance");
    }

    void SetupAllianceChatRoom(IResolutionContainer container, ChatRoom<AllianceChatMessage> room)
    {
        // Configure optional events to manage custom data
        room.ParseUnknownNotifications = AllianceChatMessage.ParseUnknownNotifications;
        room.ParseExtraInfo = AllianceChatMessage.ParseExtraInfo;
        room.SerializeExtraInfo = AllianceChatMessage.SerializeExtraInfo;
    }
}
