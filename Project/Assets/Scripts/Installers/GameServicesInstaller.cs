using System;
using SocialPoint.AppEvents;
using SocialPoint.CrossPromotion;
using SocialPoint.Dependency;
using SocialPoint.ServerSync;
using SocialPoint.ServerEvents;
using SocialPoint.Notifications;
using SocialPoint.Purchase;
using SocialPoint.Utils;

public class GameServicesInstaller : Installer
{
    public enum NetworkTech
    {
        Local,
        Unet,
        Photon
    }

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
}
