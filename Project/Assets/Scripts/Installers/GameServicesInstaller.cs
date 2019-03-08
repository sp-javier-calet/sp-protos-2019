using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.CrossPromotion;
using SocialPoint.Dependency;
using SocialPoint.GameLoading;
using SocialPoint.Locale;
using SocialPoint.Notifications;
using SocialPoint.Social;

public class GameServicesInstaller : Installer, IInitializable
{
    public override void InstallBindings(IBindingContainer container)
    {
        container.Bind<IInitializable>().ToInstance(this);

        // Purchase store // TODO IVAN
        //container.Bind<IStoreProductSource>().ToGetter<ConfigModel>((Config) => Config.Store);

        // Social Framework - Game chat rooms
        container.Bind<ChatRoom<PublicChatMessage>>().ToMethod(CreatePublicChatRoom);
        container.Bind<IChatRoom>().ToLookup<ChatRoom<PublicChatMessage>>();
        container.Listen<ChatRoom<PublicChatMessage>>().Then(SetupPublicChatRoom);
        container.Bind<ChatRoom<AllianceChatMessage>>().ToMethod(CreateAllianceChatRoom);
        container.Bind<IChatRoom>().ToLookup<ChatRoom<AllianceChatMessage>>();
        container.Listen<ChatRoom<AllianceChatMessage>>().Then(SetupAllianceChatRoom);
    }

    public void Initialize(IResolutionContainer container)
    {
        SetupNotificationsProvider(container);
        SetupCrossPromotionManager(container);
        SetupGameLoadingOperations(container);
    }

    static void SetupNotificationsProvider(IResolutionContainer container)
    {
        var manager = container.Resolve<INotificationManager>();
        if(manager == null)
        {
            return;
        }

        manager.NotificationsProvider = () =>
        {
            var notify = new Notification(10, Notification.OffsetType.None)
            {
                Title = "Notification!", Message = "This is a notification manager notification."
            };

            return new List<Notification>
            {
                notify
            };
        };
    }

    static void SetupCrossPromotionManager(IResolutionContainer container)
    {
        var manager = container.Resolve<ICrossPromotionManager>();
        if(manager == null)
        {
            return;
        }

        manager.PopupController = container.Resolve<PopupsController>();
    }

    LoadingOperation _gameOperation;

    void SetupGameLoadingOperations(IResolutionContainer container)
    {
        var manager = container.Resolve<ILoadingManager>();

        //Example of how to append LoadingOperations to the LoadingManager.
        manager.GameOperationsSource = () =>
        {
            _gameOperation = new LoadingOperation(0.0f, DoGameOperation);

            return new List<ILoadingOperation>
            {
                _gameOperation
            };
        };
    }

    void DoGameOperation()
    {
        //
        // Do operation stuff here.
        //

        _gameOperation.Finish();

        Log.i("GameOperation Finished");
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
